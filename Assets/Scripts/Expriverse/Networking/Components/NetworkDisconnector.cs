using Cysharp.Threading.Tasks;
using Expriverse.Localization;
using Expriverse.UI.Popup;
using MessagePipe;
using UnityEngine;
using VContainer;
using Xoderony.Localization;

namespace Expriverse.Networking.Components {

    [DisallowMultipleComponent]
    public class NetworkDisconnector : MonoBehaviour {

        [LocalizationKey]
        public string messageKey = L10nKeys.IngameMenu.Disconnect.Message;

        [Inject] internal ConfirmPopup _confirmPopup;
        [Inject] internal ISessionService _sessionService;
        [Inject] internal IPublisher<UIStateChangedMessage> _publisher;

        public void Disconnect() {
            var message = Localizer.GetString(messageKey);
            _confirmPopup.ShowConfirm(message,
                MessageLevel.Warning,
                () => _sessionService.LeaveSessionAsync().Forget()
            );
        }
    }
}
