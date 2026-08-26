using System;
using UnityEngine;

namespace Expriverse.UI.Popup {

    [DisallowMultipleComponent]
    public sealed class LoaderPopup : Popup {
        private int _count;

        public IDisposable Show() {
            enabled = true;
            _count++;
            return new LoadingHandle(this);
        }

        private void Hide() {
            _count--;
            if (_count == 0) {
                enabled = false;
            }
        }

        private sealed class LoadingHandle : IDisposable {
            private LoaderPopup _popup;

            public LoadingHandle(LoaderPopup popup) {
                _popup = popup;
            }

            public void Dispose() {
                var popup = _popup;
                _popup = null;
                popup?.Hide();
            }
        }
    }
}
