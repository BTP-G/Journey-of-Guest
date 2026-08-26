using UnityEngine;
using VContainer;
using Xoderony.Extensions;
using Xoderony.Movement;
using Xoderony.InputChannels;

namespace Expriverse.Character.Components {

    [DefaultExecutionOrder(2)]
    public class CharacterAimInputHandler : MonoBehaviour, IComponent {
        public float aimTime;
        public float rotateSpeed;
        [Inject] internal CharacterMotor _motor;
        private InputChannel<AimInput> _aimInput;

        [Inject]
        internal void Inject(InputChannelHub inputChannelHub) {
            _aimInput = inputChannelHub.GetInputChannel<AimInput>(InputKeys.Aim);
        }

        private void FixedUpdate() {
            var currentRotation = _motor.Rotation;
            var up = currentRotation * Vector3.up;
            Vector3 forward;
            if (aimTime > 0) {
                aimTime -= Time.deltaTime;
                forward = (_aimInput.value.position - _motor.Position).ProjectOnPlane(up).normalized;
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
