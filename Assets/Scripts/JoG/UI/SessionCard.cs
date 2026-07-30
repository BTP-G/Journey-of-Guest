using Cysharp.Text;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace JoG.UI {

    public class SessionCard : Selectable, IPointerClickHandler {
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _playerCountText;
        public object Data { get; set; }

        public event Action<object> OnClick;

        public void UpdateView(string sessionName, int availableSlots, int maxPlayers) {
            _nameText.text = sessionName;
            using var sb = ZString.CreateStringBuilder(true);
            sb.Append(maxPlayers - availableSlots);
            sb.Append('/');
            sb.Append(maxPlayers);
            _playerCountText.SetText(sb);
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData) {
            if (eventData.button == PointerEventData.InputButton.Left) {
                OnClick?.Invoke(Data);
            }
        }

        //private async void JoinSession() {
        //    using (_popupManager.PopupLoader()) {
        //        try {
        //            await sessionService.JoinSessionByIdAsync(_sessionId);
        //        } catch (Exception e) {
        //            this.LogException(e);
        //            var error = Localizer.GetString(L10nKeys.Session.Join.Failed, e.Message);
        //            _popupManager.PopupMessage(error, MessageLevel.Error);
        //            return;
        //        }
        //    }
        //    var message = Localizer.GetString(L10nKeys.Session.Join.Joined, sessionService.Session.Code);
        //    GUIUtility.systemCopyBuffer = sessionService.Session.Code;
        //    _popupManager.PopupToast(message, MessageLevel.Info, ToastPosition.TopRight);
        //}
    }
}
