using UnityEngine;
using UnityEngine.Events;

namespace JoG.UI.Popup {

    public class Popup : MonoBehaviour {
        public UnityEvent enable = new();
        public UnityEvent disable = new();

        protected virtual void OnEnable() {
            enable.Invoke();
        }

        protected virtual void OnDisable() {
            disable.Invoke();
        }
    }
}
