using Cysharp.Threading.Tasks;
using Xoderony.Localization;
using JoG.UI.Popup;
using JoG.Localization;
using MessagePipe;
using UnityEngine;
using VContainer;

namespace JoG.Networking.Components {

    [DisallowMultipleComponent]
    public class NetworkDisconnector : MonoBehaviour {

        [LocalizationKey]
        public string messageKey = L10nKeys.IngameMenu.Disconnect.Message;

        [Inject] internal PopupManager _popupManager;
        [Inject] internal ISessionService _sessionService;
        [Inject] internal IPublisher<UIStateChangedMessage> _publisher;

        public void Disconnect() {
            var message = Localizer.GetString(messageKey);
            _popupManager.PopupConfirm(message,
                MessageLevel.Warning,
                () => _sessionService.LeaveSessionAsync().Forget()
            );
        }
    }
}
