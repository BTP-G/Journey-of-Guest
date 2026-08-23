using JoG.Localization;
using JoG.UI.Popup;
using UnityEngine;
using VContainer;
using Xoderony.Localization;

namespace JoG.Components {

    [DisallowMultipleComponent]
    public class ApplicationQuiter : MonoBehaviour {

        [LocalizationKey]
        public string messageKey = L10nKeys.MainMenu.Quit.Message;

        [Inject] internal ConfirmPopup _confirmPopup;

        public void QuitGame() {
            var message = Localizer.GetString(messageKey);
#if UNITY_EDITOR
            _confirmPopup.ShowConfirm(message, MessageLevel.Warning, UnityEditor.EditorApplication.ExitPlaymode);
#else
            _confirmPopup.ShowConfirm(message, MessageLevel.Warning, Application.Quit);
#endif
        }
    }
}
