using Steamworks;
using System;
using VContainer.Unity;
using Xoderony.Logging;
using SLobby = Steamworks.Data.Lobby;

namespace JoG.Networking.P2P {
    /// <summary>
    /// Steam Lobby 平台事实：仅订阅 Matchmaking，不依赖 Transport，不实现 <see cref="Xoderony.Networking.INetworkSession"/>。
    /// </summary>
    public sealed class SteamNetworkLobby : IInitializable, IDisposable {
        private SLobby _lobby;
        private ulong _ownerPeerId;

        public bool IsStarted => _lobby.Id.IsValid;

        public ulong OwnerPeerId => _ownerPeerId;

        public bool IsOwner => IsStarted && _ownerPeerId == SteamClient.SteamId;

        public SLobby Lobby => _lobby;

        public event Action Started;

        public event Action Stopped;

        public event Action<ulong> MemberJoined;

        public event Action<ulong> MemberLeft;

        public event Action<ulong, ulong> OwnerChanged;

        public event Action LobbyDataChanged;

        public event Action<Friend> LobbyMemberDataChanged;

        void IInitializable.Initialize() {
            SteamMatchmaking.OnLobbyEntered += OnLobbyEntered;
            SteamMatchmaking.OnLobbyDataChanged += OnLobbyDataChanged;
            SteamMatchmaking.OnLobbyMemberDataChanged += OnLobbyMemberDataChanged;
            SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoined;
            SteamMatchmaking.OnLobbyMemberLeave += OnLobbyMemberLeave;
        }

        public void Dispose() {
            if (IsStarted) {
                StopLobby();
            }

            SteamMatchmaking.OnLobbyEntered -= OnLobbyEntered;
            SteamMatchmaking.OnLobbyDataChanged -= OnLobbyDataChanged;
            SteamMatchmaking.OnLobbyMemberDataChanged -= OnLobbyMemberDataChanged;
            SteamMatchmaking.OnLobbyMemberJoined -= OnLobbyMemberJoined;
            SteamMatchmaking.OnLobbyMemberLeave -= OnLobbyMemberLeave;
        }

        private void OnLobbyEntered(SLobby lobby) {
            if (IsStarted && _lobby.Id == lobby.Id) {
                return;
            }

            if (IsStarted) {
                StopLobby();
            }

            StartLobby(lobby);
        }

        private void OnLobbyDataChanged(SLobby lobby) {
            if (!IsCurrentLobby(lobby)) {
                return;
            }

            var newOwnerPeerId = (ulong)lobby.Owner.Id;
            if (newOwnerPeerId != _ownerPeerId) {
                var previousOwnerPeerId = _ownerPeerId;
                _ownerPeerId = newOwnerPeerId;
                OwnerChanged?.Invoke(previousOwnerPeerId, newOwnerPeerId);
            }

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
                StopLobby();
                return;
            }

            MemberLeft?.Invoke(friend.Id);
        }

        private void StartLobby(in SLobby lobby) {
            _lobby = lobby;
            _ownerPeerId = lobby.Owner.Id;
            this.Log($"Lobby started. Lobby={lobby.Id} Owner={_ownerPeerId}");
            Started?.Invoke();
        }

        private void StopLobby() {
            var lobbyId = _lobby.Id;
            var ownerPeerId = _ownerPeerId;
            this.Log($"Lobby stopped. Lobby={lobbyId} Owner={ownerPeerId}");
            Stopped?.Invoke();
            _lobby = default;
            _ownerPeerId = 0;
        }

        private bool IsCurrentLobby(in SLobby lobby) {
            return IsStarted && _lobby.Id == lobby.Id;
        }
    }
}
