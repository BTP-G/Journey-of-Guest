using Cysharp.Threading.Tasks;
using EditorAttributes;
using JoG.Localization;
using JoG.UI;
using JoG.UI.Popup;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using Xoderony.Localization;
using Xoderony.Logging;

namespace JoG.Networking {

    public class QuerySessionsController : MonoBehaviour {
        [Inject] internal ISessionService _sessionService;
        [Inject] internal PopupManager _popupManager;
        [Inject] internal NetworkManager _networkManager;
        private readonly List<SessionCard> _sessionCards = new();
        [Required, SerializeField] private SessionCard _sessionCardTemplate;
        [Required, SerializeField] private ScrollRect _sessionCardsView;
        [Required, SerializeField] private Button _refreshButton;
        private IDisposable _loader;

        public async void Refresh() {
            using (_popupManager.PopupLoader()) {
                var sessionsResults = await _sessionService.QuerySessions();
                var sesions = sessionsResults.Sessions;
                while (_sessionCards.Count < sesions.Count) {
                    var sessionCard = Instantiate(_sessionCardTemplate, _sessionCardsView.content);
                    sessionCard.OnClick += OnSessionCardClick;
                    _sessionCards.Add(sessionCard);
                }
                for (var i = 0; i < _sessionCards.Count; ++i) {
                    var sessionCard = _sessionCards[i];
                    if (i < sesions.Count) {
                        sessionCard.gameObject.SetActive(true);
                        var info = sesions[i];
                        sessionCard.Data = info;
                        sessionCard.UpdateView(info.Name, info.AvailableSlots, info.MaxPlayers);
                    } else {
                        sessionCard.gameObject.SetActive(false);
                    }
                }
            }
        }

        private void Awake() {
            _refreshButton.onClick.AddListener(Refresh);
        }

        private void OnEnable() {
            _networkManager.OnConnectionEvent += OnConnectionEvent;
            Refresh();
        }

        private void OnDisable() {
            _networkManager.OnConnectionEvent -= OnConnectionEvent;
            _loader?.Dispose();
        }

        private void OnConnectionEvent(NetworkManager manager, ConnectionEventData data) {
            manager.Log(data.EventType.ToString());
            if (data.EventType == ConnectionEvent.ClientConnected) {
                _sessionService.LeaveSessionAsync().Forget();
            }
            _loader?.Dispose();
            _loader = null;
        }

        private async void OnSessionCardClick(object data) {
            var info = data as ISessionInfo;
            _loader = _popupManager.PopupLoader();
            try {
                await _sessionService.JoinSessionByIdAsync(info.Id);
            } catch (Exception ex) {
                _loader?.Dispose();
                _loader = null;
                this.LogException(ex);
                var error = Localizer.GetString(L10nKeys.Session.Join.Failed, ex.Message);
                _popupManager.PopupMessage(error, MessageLevel.Error);
            }
        }
    }
}
