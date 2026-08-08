#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;

namespace ANU.IngameDebug.Console {
    public sealed class ConsoleNewInput : IConsoleInput {
        private readonly Keyboard keyboard;

        public ConsoleNewInput() {
            keyboard = Keyboard.current;
        }

        public bool GetControl() {
            return IsKey(Key.LeftCtrl) || IsKey(Key.RightCtrl);
        }

        public bool GetOpen() {
            return IsKeyDown(Key.Backquote);
        }

        public bool GetDot() {
            return keyboard[Key.Period].wasPressedThisFrame;
        }

        public bool GetUp() {
            return keyboard[Key.UpArrow].wasPressedThisFrame;
        }

        public bool GetDown() {
            return keyboard[Key.DownArrow].wasPressedThisFrame;
        }

        public bool GetTab() {
            return keyboard[Key.Tab].wasPressedThisFrame;
        }

        public bool GetEscape() {
            return keyboard[Key.Escape].wasPressedThisFrame;
        }

        private bool IsKey(Key key) {
            return keyboard[key].isPressed;
        }

        private bool IsKeyDown(Key key) {
            return keyboard[key].wasPressedThisFrame;
        }
    }
}
#endif