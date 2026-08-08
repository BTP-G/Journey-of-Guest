using JoG.States;

namespace JoG.StateMachines {

    public sealed class StateMachine {

        private IState _currentState;

        public IState CurrentState => _currentState;

        public void TransitionTo(IState state) {
            if (_currentState == state) {
                return;
            }

            var previousState = _currentState;
            _currentState = state;
            previousState?.Exit();
            state?.Enter();
        }
    }
}
