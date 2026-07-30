using UnityEngine;

namespace JoG.States {

    [DisallowMultipleComponent]
    public class State : MonoBehaviour, IComponent, IState {

        object IComponent.Key => gameObject.name;


        public void Enter() {
            enabled = true;
        }

        public void Exit() {
            enabled = false;
        }

        protected virtual void Awake() { }

        protected virtual void Reset() {
            gameObject.name = GetType()
                              .Name;
        }

    }

}
