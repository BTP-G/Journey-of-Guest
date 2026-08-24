using System;
using VContainer.Unity;
using Xoderony.Networking;
using Xoderony.Networking.Transport;

namespace JoG.Networking.P2P {
    /// <summary>
    /// 玩法层会话：组合 <see cref="SteamNetworkLobby"/> 与 <see cref="INetworkTransport"/>。
    /// Lobby 成员变化时直接建立/断开 Transport 连接；成员进出事件来自 Transport。
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
            _lobby.MemberJoined += OnLobbyMemberJoined;
            _lobby.MemberLeft += OnLobbyMemberLeft;
            _transport.PeerConnected += OnPeerConnected;
            _transport.PeerDisconnected += OnPeerDisconnected;
        }

        public void Dispose() {
            _lobby.Started -= OnLobbyStarted;
            _lobby.Stopped -= OnLobbyStopped;
            _lobby.OwnerChanged -= OnLobbyOwnerChanged;
            _lobby.MemberJoined -= OnLobbyMemberJoined;
            _lobby.MemberLeft -= OnLobbyMemberLeft;
            _transport.PeerConnected -= OnPeerConnected;
            _transport.PeerDisconnected -= OnPeerDisconnected;
        }

        private void OnLobbyStarted() {
            foreach (var member in _lobby.Lobby.Members) {
                if (!member.IsMe) {
                    _transport.ConnectPeer(member.Id);
                }
            }

            Started?.Invoke();
        }

        private void OnLobbyStopped() {
            DisconnectRemotes();
            Stopped?.Invoke();
        }

        private void OnLobbyOwnerChanged(ulong previousOwnerPeerId, ulong newOwnerPeerId) {
            OwnerChanged?.Invoke(previousOwnerPeerId, newOwnerPeerId);
        }

        private void OnLobbyMemberJoined(ulong peerId) {
            _transport.ConnectPeer(peerId);
        }

        private void OnLobbyMemberLeft(ulong peerId) {
            _transport.DisconnectPeer(peerId);
        }

        private void OnPeerConnected(ulong peerId) {
            MemberJoined?.Invoke(peerId);
        }

        private void OnPeerDisconnected(ulong peerId) {
            MemberLeft?.Invoke(peerId);
        }

        private void DisconnectRemotes() {
            if (!_lobby.IsStarted) {
                return;
            }

            foreach (var member in _lobby.Lobby.Members) {
                if (!member.IsMe) {
                    _transport.DisconnectPeer(member.Id);
                }
            }
        }
    }
}
