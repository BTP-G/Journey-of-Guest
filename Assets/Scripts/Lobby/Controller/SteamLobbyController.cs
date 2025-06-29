using JoG.DebugExtensions;
using Netcode.Transports.Facepunch;
using Steamworks;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using SLobby = Steamworks.Data.Lobby;

namespace JoG.Lobby.Controller {

    public class SteamLobbyController : MonoBehaviour {
        private SLobby currentLobby;
        [SerializeField] private NetworkManager networkManager;
        [SerializeField] private FacepunchTransport facepunchTransport;
        public SLobby Lobby => currentLobby;
        public string LobbyId => currentLobby.Id.ToString();
        public IEnumerable<Friend> Members => currentLobby.Members;
        public int MemberCount => currentLobby.MemberCount;
        public bool IsServer => currentLobby.Owner.IsMe;

        public string LobbyName {
            get => currentLobby.GetData("name");
            set => currentLobby.SetData("name", value);
        }

        public byte MaxMembers {
            get => (byte)currentLobby.MaxMembers;
            set {
                currentLobby.MaxMembers = value; 
                //facepunchTransport.cl(value);
            }
        }

        public ELobbyType LobbyType {
            get => byte.TryParse(currentLobby.GetData("type"), out var typeIndex) ? (ELobbyType)typeIndex : ELobbyType.Private;
            set {
                switch (value) {
                    case ELobbyType.Public:
                        currentLobby.SetPublic();
                        break;

                    case ELobbyType.FriendsOnly:
                        currentLobby.SetFriendsOnly();
                        break;

                    case ELobbyType.Private:
                    default:
                        currentLobby.SetPrivate();
                        break;
                }
                currentLobby.SetJoinable(true);
                currentLobby.SetData("type", ((byte)value).ToString());
            }
        }

        public void SetLobbyName(string lobbyName) => LobbyName = lobbyName;

        public void SetLobbyMaxMembersFromString(string maxMembersString) {
            if (byte.TryParse(maxMembersString, out var maxMembers)) {
                MaxMembers = maxMembers;
            }
        }

        public void SetLobbyTypeFromInt(int lobbyType) => LobbyType = (ELobbyType)lobbyType;

        public void LeaveCurrentLobby() => currentLobby.Leave();

        public void OpenInviteFriendsUI() {
            SteamFriends.OpenGameInviteOverlay(currentLobby.Id);
        }

        public void SetGameServer() => currentLobby.SetGameServer(SteamClient.SteamId);

        public bool GetGameServer(out SteamId serverId) {
            var ip = 0u;
            var port = default(ushort);
            serverId = default;
            return currentLobby.GetGameServer(ref ip, ref port, ref serverId);
        }

        public void SetLobbyData(string key, string value) => currentLobby.SetData(key, value);

        public string GetLobbyData(string key) => currentLobby.GetData(key);

        private void Awake() {
            SteamMatchmaking.OnLobbyCreated += OnLobbyCreated;
            SteamMatchmaking.OnLobbyEntered += OnLobbyEntered;
            SteamMatchmaking.OnLobbyInvite += OnLobbyInvite;
            SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoined;
            SteamMatchmaking.OnLobbyMemberLeave += OnLobbyMemberLeave;
            SteamFriends.OnGameLobbyJoinRequested += OnGameLobbyJoinRequested;
        }

        private void OnDestroy() {
            SteamMatchmaking.OnLobbyCreated -= OnLobbyCreated;
            SteamMatchmaking.OnLobbyEntered -= OnLobbyEntered;
            SteamMatchmaking.OnLobbyInvite -= OnLobbyInvite;
            SteamMatchmaking.OnLobbyMemberJoined -= OnLobbyMemberJoined;
            SteamMatchmaking.OnLobbyMemberLeave -= OnLobbyMemberLeave;
            SteamFriends.OnGameLobbyJoinRequested -= OnGameLobbyJoinRequested;
        }

        private void OnApplicationQuit() {
            LeaveCurrentLobby();
        }

        private void OnTransportEvent(NetworkEvent eventType, ulong clientId, System.ArraySegment<byte> payload, float receiveTime) {
            switch (eventType) {
                case NetworkEvent.Data:
                    break;

                case NetworkEvent.Connect:
                    break;

                case NetworkEvent.Disconnect:
                    break;

                case NetworkEvent.TransportFailure:
                    break;

                case NetworkEvent.Nothing:
                    break;

                default:
                    break;
            }
        }

        //private async void OnServerConnectionState(ServerConnectionStateArgs args) {
        //    if (args.ConnectionState is LocalConnectionState.Started) {
        //        await SteamMatchmaking.CreateLobbyAsync(facepunchTransport.GetMaximumClients());
        //    } else if (args.ConnectionState is LocalConnectionState.Stopped) {
        //        LeaveCurrentLobby();
        //    }
        //}

        //private void OnClientConnectionState(ClientConnectionStateArgs args) {
        //    if (args.ConnectionState is LocalConnectionState.Stopped) {
        //        LeaveCurrentLobby();
        //    }
        //}

        private async void OnLobbyCreated(Result result, SLobby lobby) {
            if (result is not Result.OK) return;
            lobby.SetData("app_id", SteamClient.AppId.ToString());
            //lobby.SetData("port", facepunchTransport.GetPort().ToString());
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
            if (currentLobby.Id.IsValid) {
                this.Log("You are already in a lobby!");
                return;
            }
            await lobby.Join();
        }

        private void OnLobbyEntered(SLobby lobby) {
            if (currentLobby.Id.IsValid) {
                currentLobby.Leave();
                this.Log("Leaved lobby: {0}", currentLobby.Id);
            }
            this.Log("Joined lobby: {0}", lobby.Id);
            currentLobby = lobby;
            if (!lobby.Owner.IsMe) {
                //multipass.SwitchTransport(1);
                //facepunchTransport.SetIpAddress(lobby.Owner.Id.ToString());
                //facepunchTransport.SetPortByString(ushort.Parse(lobby.GetData("port")));
                //facepunchTransport.StartConnection(false);
            }
        }

        private void OnLobbyMemberJoined(SLobby lobby, Friend friend) {
        }

        private void OnLobbyMemberLeave(SLobby lobby, Friend friend) {
            if (lobby.Id == currentLobby.Id && friend.IsMe) {
                currentLobby = default;
            }
        }
    }
}