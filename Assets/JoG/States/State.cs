using JoG.StateMachines;
using System;
using UnityEditor;
using UnityEngine;

namespace JoG.States {

    public abstract class State : MonoBehaviour, IState {
        [SerializeField] protected State[] nextStates = Array.Empty<State>();
        [field: SerializeField] public byte TransitonPriority { get; private set; } = 128;
        public IStateMachine StateMachine { get; private set; }

        void IState.Enter() => enabled = true;

        void IState.Exit() => enabled = false;

        protected virtual bool CheckTransitionIn() => true;

        protected virtual bool CheckTransitionOut() => true;

        protected void TransitionTo(IState state) => StateMachine.TransitionTo(state);

        protected virtual void Awake() {
            StateMachine = GetComponentInParent<IStateMachine>();
        }

        protected virtual void Update() {
            if (CheckTransitionOut()) {
                var nextState = default(State);
                foreach (var state in new ReadOnlySpan<State>(nextStates)) {
                    if (nextState is null || state.TransitonPriority > nextState.TransitonPriority) {
                        if (state.CheckTransitionIn()) {
                            nextState = state;
                        }
                    }
                }
                if (nextState is not null) {
                    TransitionTo(nextState);
                }
            }
        }

        protected virtual void OnValidate() {
            if (EditorApplication.isPlaying) return;
            enabled = false;
        }
    }
}