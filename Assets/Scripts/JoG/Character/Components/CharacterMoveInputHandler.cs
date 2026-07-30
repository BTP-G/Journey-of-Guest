using Xoderony.Extensions;
using Xoderony.Movement;
using JoG.Character.InputBanks;
using UnityEngine;
using VContainer;

namespace JoG.Character.Components {

    [DefaultExecutionOrder(1)]
    public class CharacterMoveInputHandler : MonoBehaviour, IComponent {

        [Inject]
        internal CharacterMotor _motor;

        [Inject]
        [Key(Constants.Stats.MaxMoveSpeed)]
        internal FloatStat _maxMoveSpeed;

        [Inject]
        [Key(Constants.Stats.MoveAcceleration)]
        internal FloatStat _moveAcceleration;

        [Inject]
        internal Animator _animator;

        private MoveInputBank _moveInput;

        [Inject]
        internal void Inject(InputBankHub inputBankHub) {
            _moveInput = inputBankHub.GetInputBank<MoveInputBank>();
        }

        private void FixedUpdate() {
            var stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            var up = _motor.Up;
            var moveDirection = _moveInput.vector3.ProjectOnPlane(up).normalized;
            var maxMoveSpeed = _maxMoveSpeed.Value;
            var targetVelocity = moveDirection * maxMoveSpeed;
            var a = _moveAcceleration.Value;
            var dt = Time.fixedDeltaTime;
            if (stateInfo.tagHash == AnimatorHashs.Ground) { } else if (stateInfo.tagHash == AnimatorHashs.Air) {
                a *= 0.2f;
            } else {
                targetVelocity = Vector3.zero;
            }
            _motor.InputVelocity = _motor.InputVelocity.MoveTowards(targetVelocity, dt * a);
        }

        private void OnDisable() {
            _motor.ExternalVelocity = _motor.InputVelocity;
            _motor.InputVelocity = Vector3.zero;
        }

        private void Update() {
            var maxMoveSpeed = _maxMoveSpeed.Value;
            var normalizedLocalVelocity = _motor.LocalVelocity / maxMoveSpeed;
            _animator.SetBool(AnimatorHashs.isGrounded, _motor.IsGrounded);
            _animator.SetFloat(AnimatorHashs.rightSpeed, normalizedLocalVelocity.x);
            _animator.SetFloat(AnimatorHashs.upSpeed, normalizedLocalVelocity.y);
            _animator.SetFloat(AnimatorHashs.forwardSpeed, normalizedLocalVelocity.z);
            _animator.SetFloat(AnimatorHashs.maxMoveSpeed, maxMoveSpeed);
        }

    }

}
