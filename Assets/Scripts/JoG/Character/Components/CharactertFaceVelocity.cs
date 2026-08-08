using UnityEngine;
using VContainer;
using Xoderony.Movement;

namespace JoG.Character.Components {

    [DefaultExecutionOrder(2)]
    public class CharactertFaceVelocity : MonoBehaviour, IComponent {
        public float rotateSpeed;
        [Inject] internal CharacterMotor _motor;

        private void FixedUpdate() {
            if (_motor.InputVelocity.sqrMagnitude > 0.0001f) {
                var up = _motor.Up;
                var desiredRotation = Quaternion.LookRotation(_motor.InputVelocity, up);
                var rotation = Quaternion.RotateTowards(_motor.Rotation, desiredRotation, Time.fixedDeltaTime * rotateSpeed);
                _motor.MoveRotation(rotation);
            }
        }
    }
}
