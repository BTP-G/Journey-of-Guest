using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Expriverse.UI.Popup {

    [DisallowMultipleComponent]
    public sealed class MessagePopup : Popup {
        public Button confirmButton;
        public TextMeshProUGUI messageText;
        public Image[] colorImages;
        private Action confirmAction;

        public void Show(string message, Color color, Action onConfirm) {
            messageText.text = message;
            confirmAction = onConfirm;
            foreach (var colorImage in colorImages) {
                colorImage.color = color;
            }
            enabled = true;
        }

        protected override void OnDisable() {
            base.OnDisable();
            confirmAction = null;
        }

        private void Awake() {
            confirmButton.onClick.AddListener(Confirm);
        }

        private void Confirm() {
            confirmAction?.Invoke();
            enabled = false;
        }
    }
}
