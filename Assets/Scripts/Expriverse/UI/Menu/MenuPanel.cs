using UnityEngine;

namespace Expriverse.UI.Menu {

    public class MenuPanel : MonoBehaviour {

        public virtual void Initialize(MenuManager manager) {
        }

        internal void InvokeOnOpen() {
            OnOpen();
        }

        internal void InvokeOnClose() {
            OnClose();
        }

        protected virtual void OnOpen() {
        }

        protected virtual void OnClose() {
        }
    }
}
