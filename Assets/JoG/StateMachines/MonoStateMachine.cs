using JoG.States;
using UnityEngine;

namespace JoG.StateMachines {

    public class MonoStateMachine : MonoBehaviour, IStateMachine {
        public State defaultState;
        private IState _currentState;
        private IState _nextState;

        public IState CurrentState => _currentState;

        public void TransitionTo(IState state) {
            _currentState?.Exit();
            if (state is null) {
                _currentState = null;
            } else {
                state.Enter();
                _currentState = state;
            }
        }

        protected void OnEnable() => TransitionTo(defaultState);

        protected void OnDisable() => TransitionTo(null);
    }
}