using Expriverse.Networking.P2P;
using Steamworks;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using Xoderony.Logging;
using SLobby = Steamworks.Data.Lobby;

namespace Expriverse.Lobby.Controller {

    /// <summary>大厅配置与 UI 命令；平台进出由本类发起，Lobby 事实由 <see cref="SteamNetworkLobby"/> 收敛。</summary>
    public class SteamLobbyController : MonoBehaviour {
        private SteamNetworkLobby _lobby;
        private SLobby _leaveLobby;

        public SLobby Lobby => _lobby.Lobby;
        public string LobbyId => Lobby.Id.ToString();
        public IEnumerable<Friend> Members => Lobby.Members;
        public int MemberCount => Lobby.MemberCount;
        public bool IsOwner => _lobby.IsOwner;

        public string LobbyName {
            get => Lobby.GetData("name");
            set => Lobby.SetData("name", value);
        }

        public byte MaxMembers {
            get => (byte)Lobby.MaxMembers;
            set {
                var lobby = Lobby;
                lobby.MaxMembers = value;
            }
        }

        public ELobbyType LobbyType {
            get => Lobby.TryGetByte("type", out var typeIndex) ? (ELobbyType)typeIndex : ELobbyType.Private;
            set {
                var lobby = Lobby;
                switch (value) {
                    case ELobbyType.Public:
                        lobby.SetPublic();
                        break;

                    case ELobbyType.FriendsOnly:
                        lobby.SetFriendsOnly();
                        break;

                    case ELobbyType.Private:
                    default:
                        lobby.SetPrivate();
                        break;
                }
                lobby.SetJoinable(true);
                lobby.SetByte("type", (byte)value);
            }
        }

        public void SetLobbyName(string lobbyName) {
            LobbyName = lobbyName;
        }

        public void SetLobbyMaxMembersFromString(string maxMembersString) {
            if (byte.TryParse(maxMembersString, out var maxMembers)) {
                MaxMembers = maxMembers;
            }
        }

        public void SetLobbyTypeFromInt(int lobbyType) {
            LobbyType = (ELobbyType)lobbyType;
        }

        public void LeaveCurrentLobby() {
            if (!_leaveLobby.Id.IsValid) {
                return;
            }

            var lobby = _leaveLobby;
            _leaveLobby = default;
            lobby.Leave();
        }

        public void OpenInviteFriendsUI() {
            SteamFriends.OpenGameInviteOverlay(Lobby.Id);
        }

        public void SetGameServer() {
            Lobby.SetGameServer(SteamClient.SteamId);
        }

        public bool GetGameServer(out SteamId serverId) {
            var ip = 0u;
            var port = default(ushort);
            serverId = default;
            return Lobby.GetGameServer(ref ip, ref port, ref serverId);
        }

        public void SetLobbyData(string key, string value) {
            Lobby.SetData(key, value);
        }

        public string GetLobbyData(string key) {
            return Lobby.GetData(key);
        }

        [Inject]
        private void Construct(SteamNetworkLobby lobby) {
            _lobby = lobby;
        }

        private void Awake() {
            SteamMatchmaking.OnLobbyCreated += OnLobbyCreated;
            SteamMatchmaking.OnLobbyInvite += OnLobbyInvite;
            SteamFriends.OnGameLobbyJoinRequested += OnGameLobbyJoinRequested;
        }

        private void Start() {
            _lobby.Started += OnLobbyStarted;
        }

        private void OnApplicationQuit() {
            LeaveCurrentLobby();
        }

        private void OnDestroy() {
            LeaveCurrentLobby();
            if (_lobby != null) {
                _lobby.Started -= OnLobbyStarted;
            }

            SteamMatchmaking.OnLobbyCreated -= OnLobbyCreated;
            SteamMatchmaking.OnLobbyInvite -= OnLobbyInvite;
            SteamFriends.OnGameLobbyJoinRequested -= OnGameLobbyJoinRequested;
        }

        private void OnLobbyStarted() {
            // 保留句柄供销毁时 Leave：Lobby 可能先 Stop，不能依赖其 Lobby。
            _leaveLobby = _lobby.Lobby;
        }

        private async void OnLobbyCreated(Result result, SLobby lobby) {
            if (result is not Result.OK) {
                return;
            }

            lobby.SetData("app_id", SteamClient.AppId.ToString());
            lobby.SetData("_inputName", SteamClient.Name + "'s Lobby");
            lobby.SetInt("difficulty", PlayerPrefs.GetInt("difficulty"));
            lobby.SetInt("mode", PlayerPrefs.GetInt("mode"));
            await SteamNetworkingUtils.WaitForPingDataAsync();
            lobby.SetData("ping_location", SteamNetworkingUtils.LocalPingLocation.GetValueOrDefault().ToString());
        }

        private void OnLobbyInvite(Friend friend, SLobby lobby) {
            this.Log($"You got invited by {friend.Name} to join {lobby.Id}");
        }

        private async void OnGameLobbyJoinRequested(SLobby lobby, SteamId id) {
            this.Log("Attempted to join by Steam invite request.");
            if (_lobby.IsStarted) {
                this.Log("You are already in a lobby!");
                return;
            }
            await lobby.Join();
        }
    }
}
