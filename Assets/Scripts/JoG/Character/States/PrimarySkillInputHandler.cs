using EditorAttributes;
using JoG.Character.InputBanks;
using JoG.StateMachines;
using JoG.States;
using VContainer;

namespace JoG.Character.States {

    public class PrimarySkillInputHandler : MonoStateMachine, IComponent {
        [Required] public State skillState;
        private PrimarySkillInputBank _inputBank;

        [Inject]
        internal void Inject(InputBankHub inputBankHub) {
            _inputBank = inputBankHub.GetInputBank<PrimarySkillInputBank>();
        }

        private void Update() {
            //if (_inputBank.Value && CurrentState == null) {
            //    TransitionTo(skillState);
            //}
        }
    }
}
