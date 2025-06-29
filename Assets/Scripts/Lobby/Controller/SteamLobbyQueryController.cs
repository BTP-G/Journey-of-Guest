using GuestUnion;
using JoG.DebugExtensions;
using JoG.Lobby.View;
using Steamworks;
using Steamworks.Data;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SLobby = Steamworks.Data.Lobby;

namespace JoG.Lobby.Controller {

    public class SteamLobbyQueryController : MonoBehaviour {
        public Transform lobbyCardsContent;
        public SteamLobbyCard lobbyCardPrefab;
        private List<SLobby> _lobbies = new();
        private List<SteamLobbyCard> _listItems = new();
        private bool isRefreshing;
        public string NameFilter { get; set; }
        public int DifficultyFilter { get; set; }
        public int ModeFilter { get; set; }
        public int LobbyDistanceFilter { get; set; }
        public bool IsFullFilter { get; set; }
        public bool HasFriendFilter { get; set; }
        public bool IsRerverse { get; set; }
        public EOrderMode OrderMode { get; set; }

        public void SetOrderMode(int orderMode) {
            if ((int)OrderMode == orderMode) {
                IsRerverse = !IsRerverse;
            } else {
                OrderMode = (EOrderMode)orderMode;
                IsRerverse = false;
            }
            RefreshView();
        }

        public async void Refresh() {
            if (isRefreshing) return;
            isRefreshing = true;
            var lobbyQuery = SteamMatchmaking.LobbyList
                .WithKeyValue("app_id", SteamClient.AppId.ToString())
                .WithMaxResults(50);
            if (DifficultyFilter > 0) {
                lobbyQuery.WithEqual("difficulty", DifficultyFilter);
            }
            if (ModeFilter > 0) {
                lobbyQuery.WithEqual("mode", ModeFilter);
            }
            switch (LobbyDistanceFilter) {
                case 1:
                    lobbyQuery.FilterDistanceClose();
                    break;

                case 2:
                    lobbyQuery.FilterDistanceFar();
                    break;

                case 3:
                    lobbyQuery.FilterDistanceWorldwide();
                    break;
            }
            lobbyQuery.WithSlotsAvailable(IsFullFilter ? 0 : 1);
            _lobbies.Clear();
            var result = await lobbyQuery.RequestAsync();
            if (result?.Length > 0) {
                _lobbies.AddRange(result);
            }
            if (!NameFilter.IsNullOrEmpty()) {
                _lobbies.RemoveAll(lobby => !lobby.GetData("_inputName").Contains(NameFilter));
            }
            if (HasFriendFilter) {
                _lobbies.RemoveAll(lobby => !lobby.Members.Any(f => f.IsFriend));
            }
            this.Log("Lobbies found: {0}", _lobbies.Count);
            await SteamNetworkingUtils.WaitForPingDataAsync();
            RefreshView();
            isRefreshing = false;
        }

        public void RefreshView() {
            switch (OrderMode) {
                case EOrderMode.ByName:
                    _lobbies.Sort((a, b) => a.GetData("_inputName").CompareTo(b.GetData("_inputName")));
                    break;

                case EOrderMode.ByPlayerCount:
                    _lobbies.Sort((a, b) => a.MemberCount.CompareTo(b.MemberCount));
                    break;

                case EOrderMode.ByDifficulty:
                    _lobbies.Sort((a, b) => a.GetData("difficulty").CompareTo(b.GetData("difficulty")));
                    break;

                case EOrderMode.ByMode:
                    _lobbies.Sort((a, b) => a.GetData("mode").CompareTo(b.GetData("mode")));
                    break;

                case EOrderMode.ByPing:
                    _lobbies.Sort((a, b) => {
                        var pingLocationA = NetPingLocation.TryParseFromString(a.GetData("ping_location"));
                        var pingLocationB = NetPingLocation.TryParseFromString(b.GetData("ping_location"));
                        if (pingLocationA.HasValue && pingLocationB.HasValue) {
                            return SteamNetworkingUtils.EstimatePingTo(pingLocationA.Value).CompareTo(SteamNetworkingUtils.EstimatePingTo(pingLocationB.Value));
                        }
                        return 0;
                    });
                    break;
            }
            if (IsRerverse) {
                _lobbies.Reverse();
            }
            while (_listItems.Count < _lobbies.Count) {
                _listItems.Add(Instantiate(lobbyCardPrefab, lobbyCardsContent));
            }
            for (int i = 0; i < _listItems.Count; i++) {
                if (i < _lobbies.Count) {
                    _listItems[i].gameObject.SetActive(true);
                    _listItems[i].UpdateView(_lobbies[i]);
                } else {
                    _listItems[i].gameObject.SetActive(false);
                }
            }
        }

        private void OnEnable() {
            Refresh();
        }
    }
}