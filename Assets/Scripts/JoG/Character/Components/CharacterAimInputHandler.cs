using JoG.Character.InputBanks;
using UnityEngine;
using VContainer;
using Xoderony.Extensions;
using Xoderony.Movement;

namespace JoG.Character.Components {

    [DefaultExecutionOrder(2)]
    public class CharacterAimInputHandler : MonoBehaviour, IComponent {
        public float aimTime;
        public float rotateSpeed;
        [Inject] internal CharacterMotor _motor;
        private AimInputBank _aimInput;

        [Inject]
        internal void Inject(InputBankHub inputBankHub) {
            _aimInput = inputBankHub.GetInputBank<AimInputBank>();
        }

        private void FixedUpdate() {
            var currentRotation = _motor.Rotation;
            var up = currentRotation * Vector3.up;
            Vector3 forward;
            if (aimTime > 0) {
                aimTime -= Time.deltaTime;
                forward = (_aimInput.vector3 - _motor.Position).ProjectOnPlane(up).normalized;
            } else {
                forward = _motor.InputVelocity.normalized;
            }
            if (forward.IsZero()) {
                return;
            }

            var targetRotation = Quaternion.LookRotation(forward, up);
            var rotation = Quaternion.Slerp(currentRotation, targetRotation, 10 * Time.deltaTime);
            _motor.MoveRotation(rotation);
        }
    }
}
