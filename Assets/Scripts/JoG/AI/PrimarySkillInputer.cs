using EditorAttributes;
using JoG.Character;
using UnityEngine;
using UnityEngine.AI;
using VContainer;
using Xoderony.InputChannels;

namespace JoG.AI {

    public class PrimarySkillInputer : MonoBehaviour, IComponent, ICharacterInputDriver {
        public float minTriggerDistance;
        [Required] public Transform aimOrigin;
        [Inject] internal AITarget target;
        private NavMeshAgent _agent;
        private InputChannel<bool> _inputChannel;

        [Inject]
        internal void Inject(NavMeshAgentController agentController) {
            _agent = agentController.agent;
        }

        void ICharacterInputDriver.Bind(CharacterEntity character) {
            _inputChannel = character.InputChannelHub.GetInputChannel<bool>(InputKeys.PrimarySkill);
            enabled = true;
        }

        void ICharacterInputDriver.Unbind() {
            enabled = false;
        }

        private void Awake() {
            enabled = false;
        }

        private void Update() {
            if (target.target != null && target.target.TryGetComponent<Collider>(out var collider)) {
                var aimRay = new Ray(aimOrigin.position, target.target.position - aimOrigin.position);
                var isInRange = collider.Raycast(aimRay, out _, minTriggerDistance)
                    && _agent.remainingDistance < minTriggerDistance;
                _inputChannel.value = isInRange;
            } else {
                _inputChannel.value = false;
            }
        }
    }
}
