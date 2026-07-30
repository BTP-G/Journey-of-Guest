using EditorAttributes;
using JoG.Character;
using JoG.Character.InputBanks;
using UnityEngine;
using UnityEngine.AI;
using VContainer;

namespace JoG.AI {

    public class PrimarySkillInputer : MonoBehaviour, IComponent {
        public float minTriggerDistance;
        [Required] public Transform aimOrigin;
        [Inject] internal AITarget target;
        private NavMeshAgent _agent;
        private PrimarySkillInputBank _inputBank;

        [Inject]
        internal void Inject(InputBankHub inputBankHub, NavMeshAgentController agentController) {
            _inputBank = inputBankHub.GetInputBank<PrimarySkillInputBank>();
            _agent = agentController.agent;
        }

        private void Update() {
            if (target.target != null && target.target.TryGetComponent<Collider>(out var collider)) {
                var aimRay = new Ray(aimOrigin.position, target.target.position - aimOrigin.position);
                var isInRange = collider.Raycast(aimRay, out _, minTriggerDistance)
                    && _agent.remainingDistance < minTriggerDistance;
                _inputBank.UpdateState(isInRange);
            } else {
                _inputBank.UpdateState(false);
            }
        }
    }
}
