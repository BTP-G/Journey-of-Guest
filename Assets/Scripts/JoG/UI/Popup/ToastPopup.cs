using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JoG.UI.Popup {

    public class ToastPopup : Popup {
        public RectTransform rectTransform;
        public TextMeshProUGUI messageText;
        public Image[] colorImages;
        private Action<ToastPopup> _hideAction;
        private float _hideTime;

        public void SetAsFirstSibling(Transform parent) {
            rectTransform.SetParent(parent, false);
            rectTransform.SetAsFirstSibling();
        }

        public void Show(string message, float duration, in Color color, Action<ToastPopup> onHidden) {
            messageText.text = message;
            _hideTime = Time.time + duration;
            _hideAction = onHidden;
            foreach (var image in colorImages) {
                image.color = color;
            }
            enabled = true;
        }

        protected override void OnEnable() {
            var sizeDelta = rectTransform.sizeDelta;
            sizeDelta.y = Mathf.Max(60, messageText.preferredHeight);
            rectTransform.sizeDelta = sizeDelta;
            base.OnEnable();
        }

        protected override void OnDisable() {
            base.OnDisable();
            rectTransform.sizeDelta = Vector2.zero;
            _hideAction?.Invoke(this);
            _hideAction = null;
        }

        private void Update() {
            enabled = Time.time < _hideTime;
        }
    }
}
