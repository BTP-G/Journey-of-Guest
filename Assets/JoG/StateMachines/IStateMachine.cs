using JoG.States;

namespace JoG.StateMachines {

    public interface IStateMachine {
        IState CurrentState { get; }

        void TransitionTo(IState state);
    }
}