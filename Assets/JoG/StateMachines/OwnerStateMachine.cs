using JoG.States;
using Unity.Netcode;

namespace JoG.StateMachines {

    public class OwnerStateMachine : NetworkBehaviour, IStateMachine {
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

        public override void OnNetworkSpawn() {
            if (IsOwner) {
                TransitionTo(defaultState);
            }
        }

        public override void OnNetworkDespawn() {
            if (IsOwner) {
                TransitionTo(null);
            }
        }

        protected override void OnOwnershipChanged(ulong previous, ulong current) {
            if (IsOwner) {
                TransitionTo(defaultState);
            } else {
                TransitionTo(null);
            }
        }
    }
}