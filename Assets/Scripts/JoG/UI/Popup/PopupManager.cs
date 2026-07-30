using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace JoG.UI.Popup {

    [DisallowMultipleComponent]
    public sealed class PopupManager : MonoBehaviour {
        public readonly Stack<ToastPopup> _toastStack = new();

        [SerializeField] private LoaderPopup _loaderPopup;
        [SerializeField] private ConfirmPopup _confirmPopup;
        [SerializeField] private ToastPopup _toastTemplate;
        [SerializeField] private Sprite[] _iconArray;
        [SerializeField] private Color[] _colorArray;
        [SerializeField] private Transform[] _toastPositions;

        public void PopupToast(string message, MessageLevel level, ToastPosition position = ToastPosition.Left, float duration = 5f, Action<ToastPopup> onToastHidden = null) {
            if (!_toastStack.TryPop(out var toast)) {
                toast = Instantiate(_toastTemplate);
            }
            toast.SetAsFirstSibling(_toastPositions[(int)position]);
            toast.Show(message, duration, _colorArray[(int)level], OnToastHidden + onToastHidden);
        }

        public IDisposable PopupLoader() {
            _loaderPopup.Show();
            return _loaderPopup;
        }

        public void PopupConfirm(string message, MessageLevel level, Action onConfirm = null, Action onCancel = null) {
            _confirmPopup.cancelButton.gameObject.SetActive(true);
            _confirmPopup.Show(message,
                _colorArray[(int)level],
                onConfirm,
                onCancel
            );
        }

        public void PopupMessage(string message, MessageLevel level, Action onConfirm = null) {
            _confirmPopup.cancelButton.gameObject.SetActive(false);
            _confirmPopup.Show(message,
                _colorArray[(int)level],
                onConfirm,
                null
            );
        }

        public UniTask<bool> PopupConfirmAsync(string message, MessageLevel level) {
            var tcs = new UniTaskCompletionSource<bool>();
            PopupConfirm(message,
                level,
                () => tcs.TrySetResult(true),
                () => tcs.TrySetResult(false)
            );
            return tcs.Task;
        }

        public UniTask PopupMessageAsync(string message, MessageLevel level) {
            var tcs = new UniTaskCompletionSource();
            PopupMessage(message,
                level,
                () => tcs.TrySetResult()
            );
            return tcs.Task;
        }

        private void OnToastHidden(ToastPopup popup) {
            _toastStack.Push(popup);
        }

        private void Awake() {
            DontDestroyOnLoad(gameObject);
        }
    }
}
