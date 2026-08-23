using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
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
        [SerializeField] private Color[] _colorArray;

        private readonly Queue<ConfirmRequest> _confirmQueue = new();
        private Action _confirmAction;
        private Action _cancelAction;
        private ConfirmRequest _activeConfirm;

        public void ShowConfirm(string message, MessageLevel level, Action onConfirm = null, Action onCancel = null) {
            EnqueueConfirm(new ConfirmRequest(
                message,
                GetColor(level),
                onConfirm,
                onCancel,
                showCancel: true));
        }

        public void ShowMessage(string message, MessageLevel level, Action onConfirm = null) {
            EnqueueConfirm(new ConfirmRequest(
                message,
                GetColor(level),
                onConfirm,
                onCancel: null,
                showCancel: false));
        }

        public UniTask<bool> ShowConfirmAsync(string message, MessageLevel level) {
            var completionSource = new UniTaskCompletionSource<bool>();
            EnqueueConfirm(new ConfirmRequest(
                message,
                GetColor(level),
                onConfirm: null,
                onCancel: null,
                showCancel: true,
                completionSource: completionSource));
            return completionSource.Task;
        }

        public UniTask ShowMessageAsync(string message, MessageLevel level) {
            var completionSource = new UniTaskCompletionSource();
            EnqueueConfirm(new ConfirmRequest(
                message,
                GetColor(level),
                onConfirm: null,
                onCancel: null,
                showCancel: false,
                completionSource: completionSource));
            return completionSource.Task;
        }

        private void Show(string message, in Color color, Action onConfirm, Action onCancel) {
            messageText.text = message;
            _confirmAction = onConfirm;
            _cancelAction = onCancel;
            foreach (var colorImage in colorImages) {
                colorImage.color = color;
            }
            enabled = true;
        }

        protected override void OnDisable() {
            base.OnDisable();
            _confirmAction = null;
            _cancelAction = null;

            var request = _activeConfirm;
            _activeConfirm = null;
            request?.Cancel();
            ShowNextConfirm();
        }

        private void Awake() {
            confirmButton.onClick.AddListener(Confirm);
            cancelButton.onClick.AddListener(Cancel);
        }

        private void OnDestroy() {
            _activeConfirm?.Cancel();
            while (_confirmQueue.TryDequeue(out var request)) {
                request.Cancel();
            }
        }

        private void Confirm() {
            try {
                CompleteConfirm(true);
            } finally {
                enabled = false;
            }
        }

        private void Cancel() {
            try {
                CompleteConfirm(false);
            } finally {
                enabled = false;
            }
        }

        private Color GetColor(MessageLevel level) {
            return _colorArray[(int)level];
        }

        private void EnqueueConfirm(ConfirmRequest request) {
            _confirmQueue.Enqueue(request);
            ShowNextConfirm();
        }

        private void ShowNextConfirm() {
            if (_activeConfirm != null || !_confirmQueue.TryDequeue(out _activeConfirm)) {
                return;
            }

            cancelButton.gameObject.SetActive(_activeConfirm.ShowCancel);
            Show(
                _activeConfirm.Message,
                _activeConfirm.Color,
                _activeConfirm.OnConfirm,
                _activeConfirm.OnCancel);
        }

        private void CompleteConfirm(bool confirmed) {
            var request = _activeConfirm;
            if (request == null) {
                return;
            }

            if (!request.TryBeginCompletion()) {
                return;
            }

            try {
                if (confirmed) {
                    _confirmAction?.Invoke();
                } else {
                    _cancelAction?.Invoke();
                }
            } finally {
                request.SetResult(confirmed);
            }
        }

        private sealed class ConfirmRequest {
            public readonly string Message;
            public readonly Color Color;
            public readonly Action OnConfirm;
            public readonly Action OnCancel;
            public readonly bool ShowCancel;
            private readonly UniTaskCompletionSource<bool> _boolCompletionSource;
            private readonly UniTaskCompletionSource _completionSource;
            private bool _completed;

            public ConfirmRequest(
                string message,
                in Color color,
                Action onConfirm,
                Action onCancel,
                bool showCancel,
                UniTaskCompletionSource<bool> completionSource = null) {

                Message = message;
                Color = color;
                OnConfirm = onConfirm;
                OnCancel = onCancel;
                ShowCancel = showCancel;
                _boolCompletionSource = completionSource;
            }

            public ConfirmRequest(
                string message,
                in Color color,
                Action onConfirm,
                Action onCancel,
                bool showCancel,
                UniTaskCompletionSource completionSource) {

                Message = message;
                Color = color;
                OnConfirm = onConfirm;
                OnCancel = onCancel;
                ShowCancel = showCancel;
                _completionSource = completionSource;
            }

            public bool TryBeginCompletion() {
                if (_completed) {
                    return false;
                }

                _completed = true;
                return true;
            }

            public void SetResult(bool confirmed) {
                _boolCompletionSource?.TrySetResult(confirmed);
                if (confirmed) {
                    _completionSource?.TrySetResult();
                }
            }

            public void Cancel() {
                if (!TryBeginCompletion()) {
                    return;
                }

                _boolCompletionSource?.TrySetCanceled();
                _completionSource?.TrySetCanceled();
            }
        }
    }
}
