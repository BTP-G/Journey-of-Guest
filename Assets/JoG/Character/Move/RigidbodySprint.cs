using UnityEngine;

namespace JoG.Character.Move {

    [RequireComponent(typeof(RigidbodyCharacterController))]
    public class RigidbodySprint : MonoBehaviour, ISprint {
        private RigidbodyCharacterController controller;
        private bool _isSprinting;

        [SerializeField]
        private float _sprintSpeedMultiplierAddend = 0.5f;

        public bool IsSprinting {
            get => _isSprinting;
            set {
                if (_isSprinting ^ value) {
                    if (_isSprinting = value) {
                        controller.maxStableMoveSpeed.Multiplier += _sprintSpeedMultiplierAddend;
                    } else {
                        controller.maxStableMoveSpeed.Multiplier -= _sprintSpeedMultiplierAddend;
                    }
                }
            }
        }

        public float SprintSpeedMultiplierAddend {
            get => _sprintSpeedMultiplierAddend;
            set {
                if (_isSprinting) {
                    controller.maxStableMoveSpeed.Multiplier -= _sprintSpeedMultiplierAddend - value;
                }
                _sprintSpeedMultiplierAddend = value;
            }
        }

        protected void Awake() {
            controller = GetComponent<RigidbodyCharacterController>();
        }
    }
}