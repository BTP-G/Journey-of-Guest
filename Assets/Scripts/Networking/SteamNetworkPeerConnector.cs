using Steamworks;
using System;
using UnityEngine;
using VContainer.Unity;
using Xoderony.Networking.Transport;

namespace JoG.Networking.P2P {
    /// <summary>本端 Id Ready 后，向已 Ready 的远端建立出站连接；远端晚 Ready 时补连。</summary>
    public sealed class SteamNetworkPeerConnector : IInitializable, IDisposable {
        private readonly SteamNetworkSession _session;
        private readonly INetworkTransport _transport;

        private bool _localIdReady;

        public SteamNetworkPeerConnector(SteamNetworkSession session, INetworkTransport transport) {
            _session = session;
            _transport = transport;
        }

        void IInitializable.Initialize() {
            _session.Started += OnSessionStarted;
            _session.Stopped += OnSessionStopped;
            _session.MemberLeft += OnMemberLeft;
            _session.LobbyMemberDataChanged += OnLobbyMemberDataChanged;
        }

        public void Dispose() {
            _session.Started -= OnSessionStarted;
            _session.Stopped -= OnSessionStopped;
            _session.MemberLeft -= OnMemberLeft;
            _session.LobbyMemberDataChanged -= OnLobbyMemberDataChanged;

            DisconnectRemotes();
        }

        private void OnSessionStarted() {
            _localIdReady = false;

            if (IsLocalIdReady()) {
                MarkLocalIdReadyAndConnect();
            }
        }

        private void OnSessionStopped() {
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

            foreach (var member in _session.Lobby.Members) {
                if (!member.IsMe && IsMemberIdReady(member)) {
                    ConnectPeer(member.Id);
                }
            }
        }

        private void ConnectPeer(ulong peerId) {
            Debug.Assert(_transport.ConnectPeer(peerId), "Failed to start the Steam peer connection.");
        }

        private bool IsLocalIdReady() {
            return IsMemberIdReady(new Friend(SteamClient.SteamId));
        }

        private bool IsMemberIdReady(Friend member) {
            return NetworkObjectIdLobbyKeys.IsIdReady(
                _session.Lobby.GetMemberData(member, NetworkObjectIdLobbyKeys.IdReadyKey));
        }

        private void DisconnectRemotes() {
            if (_session.IsStarted) {
                foreach (var member in _session.Lobby.Members) {
                    if (!member.IsMe) {
                        _transport.DisconnectPeer(member.Id);
                    }
                }
            }

            _localIdReady = false;
        }
    }
}
