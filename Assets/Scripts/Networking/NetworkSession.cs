using System;
using VContainer.Unity;
using Xoderony.Networking;
using Xoderony.Networking.Transport;

namespace JoG.Networking.P2P {
    /// <summary>
    /// 玩法层会话：组合 <see cref="SteamNetworkLobby"/> 与 <see cref="INetworkTransport"/>。
    /// 成员进出仅订阅 Transport；离 Lobby 时由 <see cref="SteamNetworkPeerConnector"/> 主动断连并收敛为 <see cref="PeerDisconnected"/>。
    /// </summary>
    public sealed class NetworkSession : INetworkSession, IInitializable, IDisposable {
        private readonly SteamNetworkLobby _lobby;
        private readonly INetworkTransport _transport;

        public bool IsStarted => _lobby.IsStarted;

        public ulong LocalPeerId => _transport.LocalPeerId;

        public ulong OwnerPeerId => _lobby.OwnerPeerId;

        public bool IsOwner => _lobby.IsStarted && _lobby.OwnerPeerId == _transport.LocalPeerId;

        public event Action Started;

        public event Action Stopped;

        public event Action<ulong> MemberJoined;

        public event Action<ulong> MemberLeft;

        public event Action<ulong, ulong> OwnerChanged;

        public NetworkSession(SteamNetworkLobby lobby, INetworkTransport transport) {
            _lobby = lobby;
            _transport = transport;
        }

        void IInitializable.Initialize() {
            _lobby.Started += OnLobbyStarted;
            _lobby.Stopped += OnLobbyStopped;
            _lobby.OwnerChanged += OnLobbyOwnerChanged;
            _transport.PeerConnected += OnPeerConnected;
            _transport.PeerDisconnected += OnPeerDisconnected;
        }

        public void Dispose() {
            _lobby.Started -= OnLobbyStarted;
            _lobby.Stopped -= OnLobbyStopped;
            _lobby.OwnerChanged -= OnLobbyOwnerChanged;
            _transport.PeerConnected -= OnPeerConnected;
            _transport.PeerDisconnected -= OnPeerDisconnected;
        }

        private void OnLobbyStarted() {
            Started?.Invoke();
        }

        private void OnLobbyStopped() {
            Stopped?.Invoke();
        }

        private void OnLobbyOwnerChanged(ulong previousOwnerPeerId, ulong newOwnerPeerId) {
            OwnerChanged?.Invoke(previousOwnerPeerId, newOwnerPeerId);
        }

        private void OnPeerConnected(ulong peerId) {
            MemberJoined?.Invoke(peerId);
        }

        private void OnPeerDisconnected(ulong peerId) {
            MemberLeft?.Invoke(peerId);
        }
    }
}
