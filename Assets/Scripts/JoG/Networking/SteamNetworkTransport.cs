using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Steamworks;
using Steamworks.Data;
using Xoderony.Logging;
using Xoderony.Networking.Transport;

namespace JoG.Networking
{
    /// <summary>
    /// Steam P2P 传输，基于 Facepunch.Steamworks 的 SteamNetworkingSockets。
    /// 需要 Steam 客户端运行并已登录；<see cref="SteamAppId"/> 默认 480（Spacewar）用于开发测试。
    /// </summary>
    public sealed class SteamNetworkTransport : NetworkTransport, IConnectionManager, ISocketManager
    {
        private static bool s_steamInitialized;
        private static bool s_relayInitialized;

        private readonly Dictionary<ulong, Connection> _clients = new Dictionary<ulong, Connection>();
        private ConnectionManager _connectionManager;
        private SocketManager _socketManager;
        private byte[] _payloadCache = new byte[4096];
        private byte[] _sendBuffer = new byte[4096];

        /// <summary>Steam App ID，默认 480（Spacewar，开发测试用）。</summary>
        public uint SteamAppId { get; set; } = 480;

        /// <summary>作为客户端加入时，目标房主的 Steam ID。</summary>
        public ulong TargetSteamId { get; set; }

        public override ulong ServerClientId => 0;

        public override event Action<ulong> PeerConnected;
        public override event Action<ulong> PeerDisconnected;
        public override event NetworkDataReceivedHandler DataReceived;

        public override bool StartClient()
        {
            if (TargetSteamId == 0)
            {
                this.LogError("Steam transport: TargetSteamId is not set.");
                return false;
            }

            if (!EnsureSteamInitialized())
            {
                return false;
            }

            _connectionManager = SteamNetworkingSockets.ConnectRelay<ConnectionManager>(TargetSteamId);
            _connectionManager.Interface = this;
            return true;
        }

        public override bool StartServer()
        {
            if (!EnsureSteamInitialized())
            {
                return false;
            }

            _socketManager = SteamNetworkingSockets.CreateRelaySocket<SocketManager>();
            _socketManager.Interface = this;
            return true;
        }

        public override void Send(ulong clientId, ReadOnlySpan<byte> payload, NetworkDelivery networkDelivery)
        {
            var sendType = networkDelivery == NetworkDelivery.Unreliable ? SendType.Unreliable : SendType.Reliable;

            EnsureSendCapacity(payload.Length);
            payload.CopyTo(_sendBuffer);

            if (clientId == ServerClientId)
            {
                _connectionManager?.Connection.SendMessage(_sendBuffer, 0, payload.Length, sendType);
            }
            else if (_clients.TryGetValue(clientId, out var connection))
            {
                connection.SendMessage(_sendBuffer, 0, payload.Length, sendType);
            }
            else
            {
                this.LogWarning($"Steam transport: dropped send to unknown client {clientId}.");
            }
        }

        public override void DisconnectRemoteClient(ulong clientId)
        {
            if (!_clients.TryGetValue(clientId, out var connection))
            {
                return;
            }

            connection.Flush();
            connection.Close();
            _clients.Remove(clientId);
        }

        public override void DisconnectLocalClient()
        {
            _connectionManager?.Connection.Close();
        }

        public override ulong GetCurrentRtt(ulong clientId) => 0;

        public override void Shutdown()
        {
            _connectionManager?.Close();
            _socketManager?.Close();
            _connectionManager = null;
            _socketManager = null;
            _clients.Clear();
        }

        public override void Poll()
        {
            if (!s_steamInitialized)
            {
                return;
            }

            SteamClient.RunCallbacks();

            if (!s_relayInitialized && SteamClient.IsValid)
            {
                SteamNetworkingUtils.InitRelayNetworkAccess();
                s_relayInitialized = true;
            }

            _connectionManager?.Receive();
            _socketManager?.Receive();
        }

        private bool EnsureSteamInitialized()
        {
            if (s_steamInitialized)
            {
                return true;
            }

            try
            {
                SteamClient.Init(SteamAppId, false);
                s_steamInitialized = true;
                return true;
            }
            catch (Exception e)
            {
                this.LogError($"Steam transport: SteamClient.Init failed for app id {SteamAppId}: {e.Message}");
                return false;
            }
        }

        private void EnsureSendCapacity(int size)
        {
            if (_sendBuffer.Length < size)
            {
                _sendBuffer = new byte[Math.Max(_sendBuffer.Length * 2, size)];
            }
        }

        private void EnsurePayloadCapacity(int size)
        {
            if (_payloadCache.Length < size)
            {
                _payloadCache = new byte[Math.Max(_payloadCache.Length * 2, size)];
            }
        }

        void IConnectionManager.OnConnecting(ConnectionInfo info)
        {
        }

        void IConnectionManager.OnConnected(ConnectionInfo info)
        {
            PeerConnected?.Invoke(ServerClientId);
        }

        void IConnectionManager.OnDisconnected(ConnectionInfo info)
        {
            PeerDisconnected?.Invoke(ServerClientId);
        }

        void IConnectionManager.OnMessage(IntPtr data, int size, long messageNum, long recvTime, int channel)
        {
            EnsurePayloadCapacity(size);
            Marshal.Copy(data, _payloadCache, 0, size);
            // Steam 收包不带投递信息（SendMessage 无 channel 重载），中继统一按 Reliable 处理。
            DataReceived?.Invoke(ServerClientId, new ReadOnlySpan<byte>(_payloadCache, 0, size), NetworkDelivery.Reliable);
        }

        void ISocketManager.OnConnecting(Connection connection, ConnectionInfo info)
        {
            connection.Accept();
        }

        void ISocketManager.OnConnected(Connection connection, ConnectionInfo info)
        {
            if (!_clients.ContainsKey(connection.Id))
            {
                _clients.Add(connection.Id, connection);
                PeerConnected?.Invoke(connection.Id);
            }
        }

        void ISocketManager.OnDisconnected(Connection connection, ConnectionInfo info)
        {
            if (_clients.Remove(connection.Id))
            {
                PeerDisconnected?.Invoke(connection.Id);
            }
        }

        void ISocketManager.OnMessage(
            Connection connection,
            NetIdentity identity,
            IntPtr data,
            int size,
            long messageNum,
            long recvTime,
            int channel)
        {
            EnsurePayloadCapacity(size);
            Marshal.Copy(data, _payloadCache, 0, size);
            // Steam 收包不带投递信息（SendMessage 无 channel 重载），中继统一按 Reliable 处理。
            DataReceived?.Invoke(connection.Id, new ReadOnlySpan<byte>(_payloadCache, 0, size), NetworkDelivery.Reliable);
        }
    }
}
