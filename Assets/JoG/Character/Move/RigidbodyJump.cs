using EditorAttributes;
using GuestUnion;
using UnityEngine;

namespace JoG.Character.Move {

    [RequireComponent(typeof(RigidbodyCharacterController))]
    public class RigidbodyJump : MonoBehaviour, IJump {
        public float acceleration = 10f;
        public float maxJumpSpeed = 5f;
        public float forcedOffTheGroundTime = 0.5f;
        [SerializeField, HideInInspector] private RigidbodyCharacterController controller;

        public float JumpHeight {
            get => maxJumpSpeed * maxJumpSpeed / (2 * controller.gravity.magnitude);
            [Button]
            set => maxJumpSpeed = (float)System.Math.Sqrt(2f * controller.gravity.magnitude * value);
        }

        public bool TryJump(in Vector3? directionOverride = null) {
            if (controller.groundStatus.IsStableOnGround) {
                var dir = directionOverride ?? controller.CharacterUp;
                var current = controller.currentVelocity.Project(dir);
                var target = maxJumpSpeed * dir;
                if (controller.VelocityFromGround.sqrMagnitude > Vector3.kEpsilonNormalSqrt) {
                    target += controller.VelocityFromGround.Project(dir);
                }
                var vc = Vector3.MoveTowards(current, target, acceleration) - current;
                controller.Rigidbody.AddForce(vc, ForceMode.VelocityChange);
                controller.ForcedOffTheGround(forcedOffTheGroundTime);
                return true;
            }
            return false;
        }

        protected void Reset() {
            controller = GetComponent<RigidbodyCharacterController>();
        }
    }
}