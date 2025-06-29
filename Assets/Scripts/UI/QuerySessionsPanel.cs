using JoG.Networking;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

namespace JoG.UI {

    public class QuerySessionsPanel : MonoBehaviour {
        public SessionCard sessionCardPrefab;
        public Transform sessionCardsContent;
        private List<SessionCard> _sessionCards = new();
        [Inject] private ISessionService _sessionService;

        public async void Refresh() {
            var sessionsResults = await _sessionService.QuerySessions();
            var sesions = sessionsResults.Sessions;
            while (_sessionCards.Count < sesions.Count) {
                var sessionCard = Instantiate(sessionCardPrefab, sessionCardsContent);
                sessionCard.sessionService = _sessionService;
                _sessionCards.Add(sessionCard);
            }
            for (int i = 0; i < _sessionCards.Count; ++i) {
                if (i < sesions.Count) {
                    _sessionCards[i].gameObject.SetActive(true);
                    _sessionCards[i].UpdateView(sesions[i]);
                } else {
                    _sessionCards[i].gameObject.SetActive(false);
                }
            }
        }

        private void OnEnable() {
            Refresh();
        }
    }
}