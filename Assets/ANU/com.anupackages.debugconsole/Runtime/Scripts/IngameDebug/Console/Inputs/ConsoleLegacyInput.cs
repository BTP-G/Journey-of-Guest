using UnityEngine;

namespace ANU.IngameDebug.Console {
    public sealed class ConsoleLegacyInput : IConsoleInput {
        public bool GetControl() {
            return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        }

        public bool GetOpen() {
            return Input.GetKeyDown(KeyCode.Tilde) || Input.GetKeyDown(KeyCode.BackQuote);
        }

        public bool GetDot() {
            return Input.GetKeyDown(KeyCode.Period);
        }

        public bool GetUp() {
            return Input.GetKeyDown(KeyCode.UpArrow);
        }

        public bool GetDown() {
            return Input.GetKeyDown(KeyCode.DownArrow);
        }

        public bool GetTab() {
            return Input.GetKeyDown(KeyCode.Tab);
        }

        public bool GetEscape() {
            return Input.GetKeyDown(KeyCode.Escape);
        }
    }
}