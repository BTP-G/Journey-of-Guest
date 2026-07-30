using JoG.States;
using Unity.Netcode;
using UnityEngine;

namespace JoG.StateMachines {

    [DisallowMultipleComponent]
    public abstract class NetworkStateMachine : NetworkBehaviour {

        private readonly StateMachine _stateMachine = new();

        protected IState CurrentState => _stateMachine.CurrentState;

        protected void TransitionTo(IState state) {
            if (IsSpawned && !HasAuthority) {
                return;
            }

            ApplyTransition(state);
        }

        protected void ApplyTransition(IState state) {
            _stateMachine.TransitionTo(state);
        }

        public override void OnNetworkDespawn() {
            ApplyTransition(null);
        }

    }

}
