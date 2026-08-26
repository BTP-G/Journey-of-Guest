using Expriverse.Character;
using UnityEngine;
using UnityEngine.AI;
using VContainer;
using Xoderony.InputChannels;

namespace Expriverse.AI {

    public class JumpInputer : MonoBehaviour, IComponent, ICharacterInputDriver {
        private NavMeshAgent _agent;
        private InputChannel<bool> _jumpInputChannel;

        [Inject]
        internal void Inject(NavMeshAgentController agentController) {
            _agent = agentController.agent;
        }

        void ICharacterInputDriver.Bind(CharacterEntity character) {
            var inputChannelHub = character.InputChannelHub;
            _jumpInputChannel = inputChannelHub.GetInputChannel<bool>(InputKeys.Jump);
            enabled = true;
        }

        void ICharacterInputDriver.Unbind() {
            enabled = false;
        }

        private void Awake() {
            enabled = false;
        }

        private void Update() {
            _jumpInputChannel.value = _agent.isOnOffMeshLink;
        }
    }
}
