using Steamworks;
using System;
using VContainer.Unity;
using Xoderony.Logging;
using Xoderony.Networking;
using SLobby = Steamworks.Data.Lobby;

namespace JoG.Networking.P2P {
    /// <summary>
    /// Steam Lobby 会话事实源：VContainer 入口中订阅 Matchmaking，将成员与 Owner 收敛为 <see cref="INetworkSession"/>。
    /// 不发起 Join/Leave；平台进出由大厅控制器等调用方负责。
    /// 对齐 Steam Lobby：进房仅 <see cref="Started"/>（已有成员由消费方读 <see cref="Lobby"/>）；
    /// 之后远端进出走 <see cref="MemberJoined"/> / <see cref="MemberLeft"/>；本人离开走 <see cref="Stopped"/>。
    /// Owner 只在 Lobby Data 回调中对照 <c>lobby.Owner</c>。
    /// </summary>
    public sealed class SteamNetworkSession : INetworkSession, IInitializable, IDisposable {
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
                StopSession();
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
                StopSession();
            }

            StartSession(lobby);
        }

        private void OnLobbyDataChanged(SLobby lobby) {
            if (!IsCurrentLobby(lobby)) {
                return;
            }

            var ownerPeerId = (ulong)lobby.Owner.Id;
            if (ownerPeerId != _ownerPeerId) {
                var previousOwnerPeerId = _ownerPeerId;
                _ownerPeerId = ownerPeerId;
                OwnerChanged?.Invoke(previousOwnerPeerId, ownerPeerId);
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
                StopSession();
                return;
            }

            MemberLeft?.Invoke(friend.Id);
        }

        private void StartSession(in SLobby lobby) {
            _lobby = lobby;
            _ownerPeerId = lobby.Owner.Id;
            this.Log($"Session started. Lobby={lobby.Id} Owner={_ownerPeerId}");
            Started?.Invoke();
        }

        private void StopSession() {
            var lobbyId = _lobby.Id;
            var ownerPeerId = _ownerPeerId;
            this.Log($"Session stopped. Lobby={lobbyId} Owner={ownerPeerId}");
            // 先 Stopped，便于观察方在 Lobby 引用仍有效时做最后一次写入。
            Stopped?.Invoke();
            _lobby = default;
            _ownerPeerId = 0;
        }

        private bool IsCurrentLobby(in SLobby lobby) {
            return IsStarted && _lobby.Id == lobby.Id;
        }
    }
}
