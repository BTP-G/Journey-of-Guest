using EditorAttributes;
using Expriverse.StateMachines;
using Expriverse.States;
using VContainer;
using Xoderony.InputChannels;

namespace Expriverse.Character.States {

    public class PrimarySkillInputHandler : MonoStateMachine, IComponent {
        [Required] public State skillState;
        private InputChannel<bool> _inputChannel;

        [Inject]
        internal void Inject(InputChannelHub inputChannelHub) {
            _inputChannel = inputChannelHub.GetInputChannel<bool>(InputKeys.PrimarySkill);
        }

        private void Update() {
            //if (_inputChannel.value && CurrentState == null) {
            //    TransitionTo(skillState);
            //}
        }
    }
}
