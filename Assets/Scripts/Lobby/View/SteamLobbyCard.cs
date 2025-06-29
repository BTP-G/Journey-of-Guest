using EditorAttributes;
using JoG.DebugExtensions;
using Steamworks;
using Steamworks.Data;
using System.Text;
using TMPro;
using UnityEngine;
using SLobby = Steamworks.Data.Lobby;

namespace JoG.Lobby.View {

    public class SteamLobbyCard : MonoBehaviour {
        private static StringBuilder _sharedStringBuilder = new();
        public SLobby ThisLobby { get; private set; }
        [field: SerializeField, Required] public TMP_Text NameText { get; private set; }
        [field: SerializeField, Required] public TMP_Text PlayerCountText { get; private set; }
        [field: SerializeField, Required] public TMP_Text DifficultyText { get; private set; }
        [field: SerializeField, Required] public TMP_Text ModeText { get; private set; }
        [field: SerializeField, Required] public TMP_Text PingText { get; private set; }

        public void UpdateView(SLobby lobby) {
            ThisLobby = lobby;
            NameText.text = ThisLobby.GetData("_inputName");
            PlayerCountText.SetText(_sharedStringBuilder.Clear()
                .Append(ThisLobby.MemberCount)
                .Append('/')
                .Append(ThisLobby.MaxMembers));
            DifficultyText.text = ThisLobby.GetData("difficulty");
            ModeText.text = ThisLobby.GetData("mode");
            var pingLocation = NetPingLocation.TryParseFromString(lobby.GetData("ping_location"));
            PingText.text = pingLocation.HasValue
                ? SteamNetworkingUtils.EstimatePingTo(pingLocation.Value).ToString()
                : "N/A";
        }

        public async void JoinLobby() {
            var result = await ThisLobby.Join();
            if (result is RoomEnter.Success) {
                this.Log("Joined lobby: {0}", ThisLobby.Id);
            } else {
                this.LogWarning("Failed to join lobby: {0}", ThisLobby.Id);
            }
            ThisLobby.Leave();
        }
    }
}