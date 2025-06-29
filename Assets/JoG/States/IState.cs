using JoG.StateMachines;

namespace JoG.States {

    public interface IState {
        IStateMachine StateMachine { get; }

        void Enter();

        void Exit();
    }
}