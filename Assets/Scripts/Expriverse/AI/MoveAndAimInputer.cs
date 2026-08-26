using Expriverse.Character;
using UnityEngine;
using UnityEngine.AI;
using VContainer;
using Xoderony.InputChannels;

namespace Expriverse.AI {

    public class MoveAndAimInputer : MonoBehaviour, IComponent, ICharacterInputDriver {
        [Inject] internal AITarget target;
        [Inject] internal Rigidbody body;
        private NavMeshAgent _agent;
        private InputChannel<Vector3> _moveInputChannel;
        private InputChannel<AimInput> _aimInputChannel;

        [Inject]
        internal void Inject(NavMeshAgentController agentController) {
            _agent = agentController.agent;
        }

        void ICharacterInputDriver.Bind(CharacterEntity character) {
            var inputChannelHub = character.InputChannelHub;
            _moveInputChannel = inputChannelHub.GetInputChannel<Vector3>(InputKeys.Move);
            _aimInputChannel = inputChannelHub.GetInputChannel<AimInput>(InputKeys.Aim);
            enabled = true;
        }

        void ICharacterInputDriver.Unbind() {
            enabled = false;
        }

        private void Awake() {
            enabled = false;
        }

        private void Update() {
            if (_agent.isOnOffMeshLink) {
                var off = _agent.currentOffMeshLinkData;
                _moveInputChannel.value = off.endPos - body.position;
            } else if (_agent.isOnNavMesh) {
                _moveInputChannel.value = _agent.desiredVelocity;
            }
            var aimTarget = target.target;
            _aimInputChannel.value = aimTarget != null
                ? new AimInput(aimTarget.position, aimTarget)
                : new AimInput(_agent.destination, null);
        }
    }
}
