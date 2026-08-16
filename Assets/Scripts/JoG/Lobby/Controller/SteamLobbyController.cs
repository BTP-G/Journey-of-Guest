using EditorAttributes;
using JoG.Networking.P2P;
using Steamworks;
using System.Collections.Generic;
using UnityEngine;
using Xoderony.Logging;
using SLobby = Steamworks.Data.Lobby;

namespace JoG.Lobby.Controller {

    public class SteamLobbyController : MonoBehaviour {
        [SerializeField, Required] private SteamNetworkSession _session;

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
            _session.Leave();
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

        private void Awake() {
            SteamMatchmaking.OnLobbyCreated += OnLobbyCreated;
            SteamMatchmaking.OnLobbyInvite += OnLobbyInvite;
            SteamFriends.OnGameLobbyJoinRequested += OnGameLobbyJoinRequested;
        }

        private void OnDestroy() {
            SteamMatchmaking.OnLobbyCreated -= OnLobbyCreated;
            SteamMatchmaking.OnLobbyInvite -= OnLobbyInvite;
            SteamFriends.OnGameLobbyJoinRequested -= OnGameLobbyJoinRequested;
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
            if (_session.IsJoined) {
                this.Log("You are already in a lobby!");
                return;
            }
            await lobby.Join();
        }
    }
}
