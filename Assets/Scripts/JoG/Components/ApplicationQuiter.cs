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

        [Inject] internal PopupManager _popupManager;

        public void QuitGame() {
            var message = Localizer.GetString(messageKey);
#if UNITY_EDITOR
            _popupManager.PopupConfirm(message, MessageLevel.Warning, UnityEditor.EditorApplication.ExitPlaymode);
#else
            _popupManager.PopupConfirm(message, MessageLevel.Warning, Application.Quit);
#endif
        }
    }
}
