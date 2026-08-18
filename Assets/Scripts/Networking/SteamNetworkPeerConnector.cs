using Steamworks;
using System;
using VContainer.Unity;
using Xoderony.Networking.Transport;

namespace JoG.Networking.P2P {
    /// <summary>本端 Id Ready 后，向已 Ready 的远端建立出站连接；远端晚 Ready 时补连。</summary>
    public sealed class SteamNetworkPeerConnector : IInitializable, IDisposable {
        private readonly SteamNetworkLobby _lobby;
        private readonly INetworkTransport _transport;

        private bool _localIdReady;

        public SteamNetworkPeerConnector(SteamNetworkLobby lobby, INetworkTransport transport) {
            _lobby = lobby;
            _transport = transport;
        }

        void IInitializable.Initialize() {
            _lobby.Started += OnLobbyStarted;
            _lobby.Stopped += OnLobbyStopped;
            _lobby.MemberLeft += OnMemberLeft;
            _lobby.LobbyMemberDataChanged += OnLobbyMemberDataChanged;
        }

        public void Dispose() {
            _lobby.Started -= OnLobbyStarted;
            _lobby.Stopped -= OnLobbyStopped;
            _lobby.MemberLeft -= OnMemberLeft;
            _lobby.LobbyMemberDataChanged -= OnLobbyMemberDataChanged;

            DisconnectRemotes();
        }

        private void OnLobbyStarted() {
            _localIdReady = false;

            if (IsLocalIdReady()) {
                MarkLocalIdReadyAndConnect();
            }
        }

        private void OnLobbyStopped() {
            DisconnectRemotes();
        }

        private void OnMemberLeft(ulong peerId) {
            _transport.DisconnectPeer(peerId);
        }

        private void OnLobbyMemberDataChanged(Friend member) {
            if (!IsMemberIdReady(member)) {
                return;
            }

            if (member.IsMe) {
                if (!_localIdReady) {
                    MarkLocalIdReadyAndConnect();
                }

                return;
            }

            if (_localIdReady) {
                ConnectPeer(member.Id);
            }
        }

        private void MarkLocalIdReadyAndConnect() {
            _localIdReady = true;

            foreach (var member in _lobby.Lobby.Members) {
                if (!member.IsMe && IsMemberIdReady(member)) {
                    ConnectPeer(member.Id);
                }
            }
        }

        private void ConnectPeer(ulong peerId) {
            _transport.ConnectPeer(peerId);
        }

        private bool IsLocalIdReady() {
            return IsMemberIdReady(new Friend(SteamClient.SteamId));
        }

        private bool IsMemberIdReady(Friend member) {
            return NetworkObjectIdLobbyKeys.IsIdReady(
                _lobby.Lobby.GetMemberData(member, NetworkObjectIdLobbyKeys.IdReadyKey));
        }

        private void DisconnectRemotes() {
            if (_lobby.IsStarted) {
                foreach (var member in _lobby.Lobby.Members) {
                    if (!member.IsMe) {
                        _transport.DisconnectPeer(member.Id);
                    }
                }
            }

            _localIdReady = false;
        }
    }
}
