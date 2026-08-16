using Steamworks;
using System;
using UnityEngine;
using Xoderony.Logging;
using Xoderony.Networking;
using SLobby = Steamworks.Data.Lobby;

namespace JoG.Networking.P2P {
    /// <summary>以 Steam Lobby 成员关系和 Owner 作为逻辑会话事实源。</summary>
    public sealed class SteamNetworkSession : MonoBehaviour, INetworkSession {
        private SLobby _lobby;
        private ulong _ownerPeerId;
        private ulong _pendingOwnerDeparturePeerId;

        public SLobby Lobby => _lobby;

        public bool IsJoined => _lobby.Id.IsValid;

        public ulong OwnerPeerId => _ownerPeerId;

        public bool IsOwner => IsJoined && _ownerPeerId == SteamClient.SteamId;

        public event Action Started;

        public event Action Stopped;

        public event Action<ulong> MemberJoined;

        public event Action<ulong> MemberLeft;

        public event Action<ulong, ulong> OwnerChanged;

        public event Action LobbyDataChanged;

        public event Action<Friend> LobbyMemberDataChanged;

        public void Leave() {
            if (!IsJoined) {
                return;
            }

            var lobby = _lobby;
            StopSession();
            lobby.Leave();
        }

        private void Awake() {
            SteamMatchmaking.OnLobbyEntered += OnLobbyEntered;
            SteamMatchmaking.OnLobbyDataChanged += OnLobbyDataChanged;
            SteamMatchmaking.OnLobbyMemberDataChanged += OnLobbyMemberDataChanged;
            SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoined;
            SteamMatchmaking.OnLobbyMemberLeave += OnLobbyMemberLeave;
        }

        private void OnApplicationQuit() {
            Leave();
        }

        private void OnDestroy() {
            Leave();
            SteamMatchmaking.OnLobbyEntered -= OnLobbyEntered;
            SteamMatchmaking.OnLobbyDataChanged -= OnLobbyDataChanged;
            SteamMatchmaking.OnLobbyMemberDataChanged -= OnLobbyMemberDataChanged;
            SteamMatchmaking.OnLobbyMemberJoined -= OnLobbyMemberJoined;
            SteamMatchmaking.OnLobbyMemberLeave -= OnLobbyMemberLeave;
        }

        private void OnLobbyEntered(SLobby lobby) {
            if (IsJoined && _lobby.Id == lobby.Id) {
                return;
            }

            if (IsJoined) {
                var previousLobby = _lobby;
                StopSession();
                previousLobby.Leave();
            }

            _lobby = lobby;
            _ownerPeerId = lobby.Owner.Id;
            this.Log($"Joined lobby: {lobby.Id}");
            Started?.Invoke();

            foreach (var member in lobby.Members) {
                if (!member.IsMe) {
                    MemberJoined?.Invoke(member.Id);
                }
            }
        }

        private void OnLobbyDataChanged(SLobby lobby) {
            if (!IsCurrentLobby(lobby)) {
                return;
            }

            ReconcileOwner(lobby);
            PublishPendingOwnerDeparture();
            LobbyDataChanged?.Invoke();
        }

        private void OnLobbyMemberDataChanged(SLobby lobby, Friend friend) {
            if (IsCurrentLobby(lobby)) {
                LobbyMemberDataChanged?.Invoke(friend);
            }
        }

        private void OnLobbyMemberJoined(SLobby lobby, Friend friend) {
            if (IsCurrentLobby(lobby) && !friend.IsMe) {
                MemberJoined?.Invoke(friend.Id);
            }
        }

        private void OnLobbyMemberLeave(SLobby lobby, Friend friend) {
            if (!IsCurrentLobby(lobby)) {
                return;
            }

            if (friend.IsMe) {
                StopSession();
                return;
            }

            var peerId = (ulong)friend.Id;
            if (peerId != _ownerPeerId) {
                MemberLeft?.Invoke(peerId);
                return;
            }

            _pendingOwnerDeparturePeerId = peerId;
            ReconcileOwner(lobby);
            PublishPendingOwnerDeparture();
        }

        private bool IsCurrentLobby(in SLobby lobby) {
            return IsJoined && _lobby.Id == lobby.Id;
        }

        private void ReconcileOwner(in SLobby lobby) {
            var ownerPeerId = (ulong)lobby.Owner.Id;
            if (ownerPeerId == _ownerPeerId) {
                return;
            }

            var previousOwnerPeerId = _ownerPeerId;
            _ownerPeerId = ownerPeerId;
            OwnerChanged?.Invoke(previousOwnerPeerId, ownerPeerId);
        }

        private void PublishPendingOwnerDeparture() {
            if (_pendingOwnerDeparturePeerId == 0 || _pendingOwnerDeparturePeerId == _ownerPeerId) {
                return;
            }

            var peerId = _pendingOwnerDeparturePeerId;
            _pendingOwnerDeparturePeerId = 0;
            MemberLeft?.Invoke(peerId);
        }

        private void StopSession() {
            var lobbyId = _lobby.Id;
            _lobby = default;
            _ownerPeerId = 0;
            _pendingOwnerDeparturePeerId = 0;
            this.Log($"Left lobby: {lobbyId}");
            Stopped?.Invoke();
        }
    }
}
