using JoG.Character;
using JoG.Character.InputBanks;
using UnityEngine;
using UnityEngine.AI;
using VContainer;

namespace JoG.AI {

    public class MoveAndAimInputer : MonoBehaviour, IComponent, ICharacterInputDriver {
        [Inject] internal AITarget target;
        [Inject] internal Rigidbody body;
        private NavMeshAgent _agent;
        private MoveInputBank _moveInputBank;
        private AimInputBank _aimInputBank;

        [Inject]
        internal void Inject(NavMeshAgentController agentController) {
            _agent = agentController.agent;
        }

        void ICharacterInputDriver.Bind(CharacterEntity character) {
            var inputBankHub = character.InputBankHub;
            _moveInputBank = inputBankHub.GetInputBank<MoveInputBank>();
            _aimInputBank = inputBankHub.GetInputBank<AimInputBank>();
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
                _moveInputBank.vector3 = off.endPos - body.position;
            } else if (_agent.isOnNavMesh) {
                _moveInputBank.vector3 = _agent.desiredVelocity;
            }
            _aimInputBank.target = target.target;
            if (target.target != null) {
                _aimInputBank.vector3 = target.target.position;
            } else {
                _aimInputBank.vector3 = _agent.destination;
            }
        }
    }
}
