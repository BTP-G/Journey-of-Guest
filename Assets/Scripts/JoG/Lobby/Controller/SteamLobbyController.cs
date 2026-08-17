using JoG.Networking.P2P;
using Steamworks;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using Xoderony.Logging;
using SLobby = Steamworks.Data.Lobby;

namespace JoG.Lobby.Controller {

    /// <summary>大厅配置与 UI 命令；平台进出由本类发起，会话事实由 <see cref="SteamNetworkSession"/> 收敛。</summary>
    public class SteamLobbyController : MonoBehaviour {
        private SteamNetworkSession _session;
        private SLobby _leaveLobby;

        public SLobby Lobby => _session.Lobby;
        public string LobbyId => Lobby.Id.ToString();
        public IEnumerable<Friend> Members => Lobby.Members;
        public int MemberCount => Lobby.MemberCount;
        public bool IsOwner => _session.IsOwner;

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
            get => byte.TryParse(Lobby.GetData("type"), out var typeIndex) ? (ELobbyType)typeIndex : ELobbyType.Private;
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
                lobby.SetData("type", ((byte)value).ToString());
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
        private void Construct(SteamNetworkSession session) {
            _session = session;
        }

        private void Awake() {
            SteamMatchmaking.OnLobbyCreated += OnLobbyCreated;
            SteamMatchmaking.OnLobbyInvite += OnLobbyInvite;
            SteamFriends.OnGameLobbyJoinRequested += OnGameLobbyJoinRequested;
        }

        private void Start() {
            _session.Started += OnSessionStarted;
        }

        private void OnApplicationQuit() {
            LeaveCurrentLobby();
        }

        private void OnDestroy() {
            LeaveCurrentLobby();
            if (_session != null) {
                _session.Started -= OnSessionStarted;
            }

            SteamMatchmaking.OnLobbyCreated -= OnLobbyCreated;
            SteamMatchmaking.OnLobbyInvite -= OnLobbyInvite;
            SteamFriends.OnGameLobbyJoinRequested -= OnGameLobbyJoinRequested;
        }

        private void OnSessionStarted() {
            // 保留句柄供销毁时 Leave：Session 可能先 StopSession，不能依赖其 Lobby。
            _leaveLobby = _session.Lobby;
        }

        private async void OnLobbyCreated(Result result, SLobby lobby) {
            if (result is not Result.OK) {
                return;
            }

            lobby.SetData("app_id", SteamClient.AppId.ToString());
            lobby.SetData("_inputName", SteamClient.Name + "'s Lobby");
            lobby.SetData("difficulty", PlayerPrefs.GetInt("difficulty").ToString());
            lobby.SetData("mode", PlayerPrefs.GetInt("mode").ToString());
            await SteamNetworkingUtils.WaitForPingDataAsync();
            lobby.SetData("ping_location", SteamNetworkingUtils.LocalPingLocation.GetValueOrDefault().ToString());
        }

        private void OnLobbyInvite(Friend friend, SLobby lobby) {
            this.Log($"You got invited by {friend.Name} to join {lobby.Id}");
        }

        private async void OnGameLobbyJoinRequested(SLobby lobby, SteamId id) {
            this.Log("Attempted to join by Steam invite request.");
            if (_session.IsStarted) {
                this.Log("You are already in a lobby!");
                return;
            }
            await lobby.Join();
        }
    }
}
