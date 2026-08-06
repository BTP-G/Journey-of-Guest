using Animancer;
using Xoderony.Movement;
using JoG.Character.InputBanks;
using UnityEngine;
using VContainer;

namespace JoG.Character.States {

    public class CharacterAirState : CharacterAnimancerState {

        [SerializeField]
        private LinearMixerTransition _animation = new();

        [SerializeField]
        [Range(0, 1)]
        private float _airControl = 0.2f;

        [Inject]
        internal CharacterMotor motor;

        [Inject]
        [Key(Constants.Stats.MaxMoveSpeed)]
        internal Stat maxMoveSpeed;

        [Inject]
        [Key(Constants.Stats.MoveAcceleration)]
        internal Stat moveAcceleration;

        private MoveInputBank _moveInput;

        [Inject]
        internal void Inject(InputBankHub inputBankHub) {
            _moveInput = inputBankHub.GetInputBank<MoveInputBank>();
        }

        protected void OnEnable() {
            Play(_animation);
        }

        protected void Update() {
            _animation.State.Parameter = Vector3.Dot(motor.Velocity, motor.Up);
        }

        protected void FixedUpdate() {
            var planarInput = Vector3.ProjectOnPlane(_moveInput.vector3, motor.Up);
            var maxMoveSpeedValue = maxMoveSpeed.Value;
            var targetVelocity = Vector3.ClampMagnitude(planarInput, 1) * maxMoveSpeedValue;
            var acceleration = moveAcceleration.Value * _airControl;
            motor.InputVelocity = Vector3.MoveTowards(
                motor.InputVelocity,
                targetVelocity,
                acceleration * Time.fixedDeltaTime
            );
        }

    }

}
