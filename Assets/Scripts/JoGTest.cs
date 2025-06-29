using JoG.DebugExtensions;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace JoG {

    public class JoGTest : MonoBehaviour {
        public InputAction action;

        private void Awake() {
            //action.started += HandleActionStateChanged;
            action.performed += HandleActionStateChanged;
            //action.canceled += HandleActionStateChanged;
            action.Enable();
        }

        private void HandleActionStateChanged(InputAction.CallbackContext context) {
            this.Log((context.control as KeyControl).keyCode);
            this.Log(context.ReadValueAsObject());
        }

        private void Coll() {
             
        }
    }
}