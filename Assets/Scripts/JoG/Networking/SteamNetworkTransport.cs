using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Steamworks;
using Steamworks.Data;
using Xoderony.Logging;
using Xoderony.Networking.Transport;

namespace JoG.Networking {
    /// <summary>
    /// Steam P2P 传输（Facepunch.Steamworks / SteamNetworkingSockets）。
    /// peerId 即 SteamID；本端始终监听（接受入站），并按需建立出站连接（<see cref="ConnectPeer"/>）。
    /// 进程级单例约定：SteamClient.Init 每进程一次，<see cref="Start"/> 仅调用一次。
    /// </summary>
    public sealed class SteamNetworkTransport : INetworkTransport, ISocketManager {
        private static bool s_steamInitialized;
        private static bool s_relayInitialized;

        private readonly Dictionary<ulong, Connection> _connections = new Dictionary<ulong, Connection>();
        private readonly List<OutgoingConnection> _outgoing = new List<OutgoingConnection>();
        private readonly List<OutgoingConnection> _pendingOutgoingRemovals = new List<OutgoingConnection>();
        private SocketManager _socketManager;
        private byte[] _payloadCache = new byte[4096];
        private byte[] _sendBuffer = new byte[4096];

        /// <summary>Steam App ID，默认 480（Spacewar，开发测试用）。</summary>
        public uint SteamAppId { get; set; } = 480;

        /// <summary>本机 SteamID；Start 成功前无效（0）。</summary>
        public ulong LocalPeerId => SteamClient.SteamId;

        public event Action<ulong> PeerConnected;
        public event Action<ulong> PeerDisconnected;
        public event NetworkDataReceivedHandler DataReceived;

        public bool Start() {
            if (!EnsureSteamInitialized()) {
                return false;
            }

            // P2P 网格：本端始终监听入站连接，出站连接由 ConnectPeer 建立。
            _socketManager = SteamNetworkingSockets.CreateRelaySocket<SocketManager>();
            _socketManager.Interface = this;
            return true;
        }

        /// <summary>按 SteamID 建立出站直连；连接建立后经 <see cref="PeerConnected"/> 上报。重复调用幂等。</summary>
        public bool ConnectPeer(ulong peerId) {
            if (peerId == 0 || !EnsureSteamInitialized()) {
                return false;
            }

            foreach (var existing in _outgoing) {
                if (existing.PeerId == peerId) {
                    return true;
                }
            }

            var manager = SteamNetworkingSockets.ConnectRelay<ConnectionManager>(peerId);
            var outgoing = new OutgoingConnection(this, peerId, manager);
            manager.Interface = outgoing;
            _outgoing.Add(outgoing);
            return true;
        }

        public void SendData(ulong peerId, ReadOnlySpan<byte> payload, NetworkDelivery delivery) {
            var sendType = delivery == NetworkDelivery.Unreliable ? SendType.Unreliable : SendType.Reliable;
            EnsureSendCapacity(payload.Length);
            payload.CopyTo(_sendBuffer);

            if (_connections.TryGetValue(peerId, out var connection)) {
                connection.SendMessage(_sendBuffer, 0, payload.Length, sendType);
            } else {
                this.LogWarning($"Steam transport: dropped send to unknown peer {peerId}.");
            }
        }

        public void DisconnectPeer(ulong peerId) {
            if (!_connections.TryGetValue(peerId, out var connection)) {
                return;
            }

            connection.Flush();
            connection.Close();
            if (_connections.Remove(peerId)) {
                // 主动断开：本地立即上报一次；后续 OnDisconnected 回调因 Remove 失败而不再重复。
                PeerDisconnected?.Invoke(peerId);
            }
        }

        public void Stop() {
            foreach (var outgoing in _outgoing) {
                outgoing.Manager.Close();
            }

            _outgoing.Clear();
            _pendingOutgoingRemovals.Clear();
            _socketManager?.Close();
            _socketManager = null;
            _connections.Clear();
        }

        public void Poll() {
            if (!s_steamInitialized) {
                return;
            }

            SteamClient.RunCallbacks();

            if (!s_relayInitialized && SteamClient.IsValid) {
                SteamNetworkingUtils.InitRelayNetworkAccess();
                s_relayInitialized = true;
            }

            // Receive 回调可能触发上层再次 ConnectPeer（遍历中新增安全）；
            // 断开产生的移除统一延迟到遍历后，避免遍历中修改集合。
            for (var i = 0; i < _outgoing.Count; i++) {
                _outgoing[i].Manager.Receive();
            }

            _socketManager?.Receive();
            ApplyPendingOutgoingRemovals();
        }

        /// <summary>Steam SDR 未提供结构化 RTT（仅 DetailedStatus 文本），维持 0。</summary>
        public ulong GetRtt(ulong peerId) => 0;

        private bool EnsureSteamInitialized() {
            if (s_steamInitialized) {
                return true;
            }

            try {
                SteamClient.Init(SteamAppId, false);
                s_steamInitialized = true;
                return true;
            } catch (Exception e) {
                this.LogError($"Steam transport: SteamClient.Init failed for app id {SteamAppId}: {e.Message}");
                return false;
            }
        }

        private void EnsureSendCapacity(int size) {
            if (_sendBuffer.Length < size) {
                _sendBuffer = new byte[Math.Max(_sendBuffer.Length * 2, size)];
            }
        }

        private void EnsurePayloadCapacity(int size) {
            if (_payloadCache.Length < size) {
                _payloadCache = new byte[Math.Max(_payloadCache.Length * 2, size)];
            }
        }

        private void OnPeerConnected(ulong steamId, Connection connection) {
            _connections[steamId] = connection;
            PeerConnected?.Invoke(steamId);
        }

        private void OnPeerDisconnected(ulong steamId) {
            if (_connections.Remove(steamId)) {
                PeerDisconnected?.Invoke(steamId);
            }
        }

        private void OnOutgoingDisconnected(OutgoingConnection outgoing) {
            // Receive 栈内不能直接改 _outgoing（Poll 正在遍历），延迟到遍历后统一移除。
            _pendingOutgoingRemovals.Add(outgoing);
            OnPeerDisconnected(outgoing.PeerId);
        }

        private void ApplyPendingOutgoingRemovals() {
            if (_pendingOutgoingRemovals.Count == 0) {
                return;
            }

            foreach (var outgoing in _pendingOutgoingRemovals) {
                _outgoing.Remove(outgoing);
            }

            _pendingOutgoingRemovals.Clear();
        }

        private void OnPeerMessage(ulong steamId, IntPtr data, int size, int channel) {
            EnsurePayloadCapacity(size);
            Marshal.Copy(data, _payloadCache, 0, size);
            // Facepunch 收包无投递信息可区分（SendMessage 无 channel 重载，收包 channel 恒为 0），
            // 统一按 Reliable 上报；上层协议消息亦默认 Reliable。
            DataReceived?.Invoke(steamId, new ReadOnlySpan<byte>(_payloadCache, 0, size), NetworkDelivery.Reliable);
        }

        void ISocketManager.OnConnecting(Connection connection, ConnectionInfo info) {
            connection.Accept();
        }

        void ISocketManager.OnConnected(Connection connection, ConnectionInfo info) {
            OnPeerConnected(info.Identity.SteamId, connection);
        }

        void ISocketManager.OnDisconnected(Connection connection, ConnectionInfo info) {
            OnPeerDisconnected(info.Identity.SteamId);
        }

        void ISocketManager.OnMessage(
            Connection connection,
            NetIdentity identity,
            IntPtr data,
            int size,
            long messageNum,
            long recvTime,
            int channel) {
            OnPeerMessage(identity.SteamId, data, size, channel);
        }

        /// <summary>
        /// 出站连接适配器：每个出站 ConnectionManager 持有独立回调，用于区分多连接；
        /// PeerId 构造时固定，避免连接未完成即断开时用 0 查找。
        /// </summary>
        private sealed class OutgoingConnection : IConnectionManager {
            private readonly SteamNetworkTransport _transport;

            public OutgoingConnection(SteamNetworkTransport transport, ulong peerId, ConnectionManager manager) {
                _transport = transport;
                PeerId = peerId;
                Manager = manager;
            }

            public ulong PeerId { get; }

            public ConnectionManager Manager { get; }

            public void OnConnecting(ConnectionInfo info) {
            }

            public void OnConnected(ConnectionInfo info) {
                _transport.OnPeerConnected(PeerId, Manager.Connection);
            }

            public void OnDisconnected(ConnectionInfo info) {
                _transport.OnOutgoingDisconnected(this);
            }

            public void OnMessage(IntPtr data, int size, long messageNum, long recvTime, int channel) {
                _transport.OnPeerMessage(PeerId, data, size, channel);
            }
        }
    }
}
