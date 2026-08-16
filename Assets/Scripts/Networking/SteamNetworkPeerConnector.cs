using System;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;
using VContainer.Unity;
using Xoderony.Networking.Transport;

namespace JoG.Networking.P2P {
    /// <summary>本端取得首个对象 id 区间后，连接进入 Lobby 时已经存在的 Peer。</summary>
    public sealed class SteamNetworkPeerConnector : IInitializable, IDisposable {
        private readonly SteamNetworkSession _session;
        private readonly INetworkTransport _transport;
        private readonly HashSet<ulong> _memberPeerIds = new HashSet<ulong>();
        private bool _connectionsStarted;

        public SteamNetworkPeerConnector(SteamNetworkSession session, INetworkTransport transport) {
            _session = session;
            _transport = transport;
        }

        void IInitializable.Initialize() {
            _session.Started += OnSessionStarted;
            _session.Stopped += OnSessionStopped;
            _session.MemberJoined += OnMemberJoined;
            _session.MemberLeft += OnMemberLeft;
            _session.LobbyMemberDataChanged += OnLobbyMemberDataChanged;

            if (_session.IsJoined) {
                OnSessionStarted();
            }
        }

        public void Dispose() {
            _session.Started -= OnSessionStarted;
            _session.Stopped -= OnSessionStopped;
            _session.MemberJoined -= OnMemberJoined;
            _session.MemberLeft -= OnMemberLeft;
            _session.LobbyMemberDataChanged -= OnLobbyMemberDataChanged;
            DisconnectMembers();
        }

        private void OnSessionStarted() {
            _connectionsStarted = false;
            _memberPeerIds.Clear();
            foreach (var member in _session.Lobby.Members) {
                if (!member.IsMe) {
                    _memberPeerIds.Add(member.Id);
                }
            }

            if (IsLocalMemberReady()) {
                StartConnections();
            }
        }

        private void OnSessionStopped() {
            DisconnectMembers();
        }

        private void OnMemberJoined(ulong peerId) {
            _memberPeerIds.Add(peerId);
        }

        private void OnMemberLeft(ulong peerId) {
            _memberPeerIds.Remove(peerId);
            _transport.DisconnectPeer(peerId);
        }

        private void OnLobbyMemberDataChanged(Friend member) {
            if (member.IsMe && !_connectionsStarted && IsMemberReady(member)) {
                StartConnections();
            }
        }

        private void StartConnections() {
            _connectionsStarted = true;
            foreach (var member in _session.Lobby.Members) {
                if (!member.IsMe && IsMemberReady(member)) {
                    Debug.Assert(_transport.ConnectPeer(member.Id), "Failed to start the Steam peer connection.");
                }
            }
        }

        private bool IsLocalMemberReady() {
            foreach (var member in _session.Lobby.Members) {
                if (member.IsMe) {
                    return IsMemberReady(member);
                }
            }

            return false;
        }

        private bool IsMemberReady(Friend member) {
            return _session.Lobby.GetMemberData(member, NetworkObjectIdLobbyData.ReadyKey) == NetworkObjectIdLobbyData.ReadyValue;
        }

        private void DisconnectMembers() {
            foreach (var peerId in _memberPeerIds) {
                _transport.DisconnectPeer(peerId);
            }

            _memberPeerIds.Clear();
            _connectionsStarted = false;
        }
    }
}
