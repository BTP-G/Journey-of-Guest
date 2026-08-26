using Expriverse.States;
using UnityEngine;

namespace Expriverse.StateMachines {

    [DisallowMultipleComponent]
    public class MonoStateMachine : MonoBehaviour, IState {

        private readonly StateMachine _stateMachine = new();

        protected IState CurrentState => _stateMachine.CurrentState;

        protected void TransitionTo(IState state) {
            _stateMachine.TransitionTo(state);
        }

        protected virtual void OnEnable() { }

        protected virtual void OnDisable() {
            TransitionTo(null);
        }

        void IState.Enter() {
            enabled = true;
        }

        void IState.Exit() {
            enabled = false;
        }
    }
}
