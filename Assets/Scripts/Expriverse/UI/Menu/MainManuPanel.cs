using Expriverse.Localization;
using Expriverse.UI.Popup;
using System;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using Xoderony.Localization;

namespace Expriverse.UI.Menu {

    public class MainManuPanel : MenuPanel {
        [SerializeField] private PanelOpenButton[] _buttons;
        [SerializeField] private Button _quitButton;
        [Inject] internal ConfirmPopup _confirmPopup;

        public override void Initialize(MenuManager manager) {
            //_quitButton.onClick.AddListener(QuitGame);
            manager.OpenPanel(this);
            foreach (var button in _buttons) {
                button.button.onClick.AddListener(() => manager.OpenPanel(button.panel));
            }
        }

        private void TryQuitGame() {
            var message = Localizer.GetString(L10nKeys.MainMenu.Quit.Message);
#if UNITY_EDITOR
            _confirmPopup.ShowConfirm(message, MessageLevel.Warning, UnityEditor.EditorApplication.ExitPlaymode);
#else
            _confirmPopup.ShowConfirm(message, MessageLevel.Warning, Application.Quit);
#endif
        }

        [Serializable]
        private class PanelOpenButton {
            public Button button;
            public MenuPanel panel;
        }
    }
}
