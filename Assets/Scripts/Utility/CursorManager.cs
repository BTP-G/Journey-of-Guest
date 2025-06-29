using UnityEngine;

namespace JoG.Utility {

    public class CursorManager : MonoBehaviour {
        public static bool lockOnFocus = true;

        private void OnEnable() {
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void OnApplicationFocus(bool focus) {
            if (lockOnFocus && focus) {
                Cursor.lockState = CursorLockMode.Locked;
            }
        }

        private void OnDisable() {
            Cursor.lockState = CursorLockMode.None;
            lockOnFocus = false;
        }
    }
}