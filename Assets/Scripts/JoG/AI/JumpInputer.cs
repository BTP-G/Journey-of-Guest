using JoG.Character;
using JoG.Character.InputBanks;
using UnityEngine;
using UnityEngine.AI;
using VContainer;

namespace JoG.AI {

    public class JumpInputer : MonoBehaviour, IComponent, ICharacterInputDriver {
        private NavMeshAgent _agent;
        private JumpInputBank _jumpInputBank;

        [Inject]
        internal void Inject(NavMeshAgentController agentController) {
            _agent = agentController.agent;
        }

        void ICharacterInputDriver.Bind(CharacterEntity character) {
            var inputBankHub = character.InputBankHub;
            _jumpInputBank = inputBankHub.GetInputBank<JumpInputBank>();
            enabled = true;
        }

        void ICharacterInputDriver.Unbind() {
            enabled = false;
        }

        private void Awake() {
            enabled = false;
        }

        private void Update() {
            _jumpInputBank.UpdateState(_agent.isOnOffMeshLink);
        }
    }
}
