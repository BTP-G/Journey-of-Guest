using UnityEngine;
using UnityUtils; 

namespace JoG {

    public class CursorManager : Singleton<CursorManager> {
        private int _showRequestCount = 0;

        /// <summary>请求显示鼠标（可多次叠加）</summary>
        public void RequestShowCursor() {
            _showRequestCount++;
            UpdateCursorState();
        }

        /// <summary>取消显示鼠标的请求</summary>
        public void ReleaseShowCursor() {
            _showRequestCount = Mathf.Max(0, _showRequestCount - 1);
            UpdateCursorState();
        }

        private void UpdateCursorState() {
            if (_showRequestCount > 0) {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            } else {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        private void OnApplicationFocus(bool focus) {
            if (focus) {
                UpdateCursorState();
            }
        }

        private void OnDisable() {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}