using UnityEngine;
using UnityUtils;

namespace JoG {

    public class CursorManager : Singleton<CursorManager> {
        protected int _showRequestCount = 1;

        /// <summary>请求显示鼠标（可多次叠加）</summary>
        public void RequestShowCursor() {
            _showRequestCount++;
            UpdateCursorState();
        }

        /// <summary>取消显示鼠标的请求</summary>
        public void ReleaseShowCursor() {
            _showRequestCount--;
            UpdateCursorState();
        }

        protected void UpdateCursorState() {
            if (_showRequestCount > 0) {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            } else {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        protected void OnApplicationFocus(bool focus) {
            if (focus) {
                UpdateCursorState();
            }
        }

        protected override void Awake() {
            base.Awake();
            UpdateCursorState();
        }

        protected void OnDestroy() {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}