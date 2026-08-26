using System;
using System.Collections.Generic;
using UnityEngine;

namespace Expriverse.UI.Popup {

    [DisallowMultipleComponent]
    public sealed class ToastPopupController : MonoBehaviour {
        private readonly Stack<ToastPopup> _toastStack = new();

        [SerializeField] private ToastPopup _toastTemplate;
        [SerializeField] private Color[] _colorArray;
        [SerializeField] private Transform[] _toastPositions;

        public void Show(string message, MessageLevel level, ToastPosition position = ToastPosition.Left, float duration = 5f, Action<ToastPopup> onToastHidden = null) {
            if (!_toastStack.TryPop(out var toast)) {
                toast = Instantiate(_toastTemplate);
            }
            toast.SetAsFirstSibling(_toastPositions[(int)position]);
            toast.Show(message, duration, _colorArray[(int)level], OnToastHidden + onToastHidden);
        }

        private void OnToastHidden(ToastPopup popup) {
            _toastStack.Push(popup);
        }
    }
}
