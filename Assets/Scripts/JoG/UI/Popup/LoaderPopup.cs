using System;
using UnityEngine;

namespace JoG.UI.Popup {

    [DisallowMultipleComponent]
    public class LoaderPopup : Popup, IDisposable {
        private int _count;

        public LoaderPopup Show() {
            enabled = true;
            _count++;
            return this;
        }

        void IDisposable.Dispose() {
            _count--;
            if (_count <= 0) {
                enabled = false;
            }
        }
    }
}
