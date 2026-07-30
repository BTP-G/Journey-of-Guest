using JoG.Character;
using JoG.Character.InputBanks;
using UnityEngine;
using UnityEngine.AI;
using VContainer;

namespace JoG.AI {

    public class JumpInputer : MonoBehaviour, IComponent {
        private NavMeshAgent _agent;
        private JumpInputBank _jumpInputBank;

        [Inject]
        internal void Inject(NavMeshAgentController agentController, InputBankHub inputBankHub) {
            _agent = agentController.agent;
            _jumpInputBank = inputBankHub.GetInputBank<JumpInputBank>();
        }

        private void Update() {
            _jumpInputBank.UpdateState(_agent.isOnOffMeshLink);
        }
    }
}
