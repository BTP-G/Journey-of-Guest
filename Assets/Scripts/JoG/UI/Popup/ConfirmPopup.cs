using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JoG.UI.Popup {

    [DisallowMultipleComponent]
    public sealed class ConfirmPopup : Popup {
        public Button confirmButton;
        public Button cancelButton;
        public TextMeshProUGUI messageText;
        public Image[] colorImages;
        private Action _confirmAction;
        private Action _cancelAction;

        public void Show(string message, in Color color, Action onConfirm, Action onCancel) {
            messageText.text = message;
            _confirmAction = onConfirm;
            _cancelAction = onCancel;
            foreach (var colorImage in colorImages) {
                colorImage.color = color;
            }
            enabled = true;
        }

        protected override void OnEnable() {
            base.OnEnable();
        }

        protected override void OnDisable() {
            base.OnDisable();
            _confirmAction = null;
            _cancelAction = null;
        }

        private void Awake() {
            confirmButton.onClick.AddListener(Confirm);
            cancelButton.onClick.AddListener(Cancel);
        }

        private void Confirm() {
            _confirmAction?.Invoke();
            enabled = false;
        }

        private void Cancel() {
            _cancelAction?.Invoke();
            enabled = false;
        }
    }
}
