using EditorAttributes;
using JoG.StateMachines;
using JoG.States;
using VContainer;
using Xoderony.InputChannels;

namespace JoG.Character.States {

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
