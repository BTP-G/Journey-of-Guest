using Animancer;
using UnityEngine;
using VContainer;
using Xoderony.Movement;
using Xoderony.InputChannels;

namespace JoG.Character.States {

    public class CharacterMoveState : CharacterAnimancerState {
        [SerializeField]
        private LinearMixerTransition _animation = new();

        [Inject]
        internal CharacterMotor motor;

        [Inject, Key(Constants.Stats.MaxMoveSpeed)]
        internal Stat maxMoveSpeed;

        [Inject, Key(Constants.Stats.MoveAcceleration)]
        internal Stat moveAcceleration;

        private InputChannel<Vector3> _moveInput;

        [Inject]
        internal void Inject(InputChannelHub inputChannelHub) {
            _moveInput = inputChannelHub.GetInputChannel<Vector3>(InputKeys.Move);
        }

        protected void OnEnable() {
            Play(_animation);
        }

        protected void Update() {
            var speed = Vector3.ProjectOnPlane(motor.Velocity, motor.Up).magnitude;
            var maxMoveSpeedValue = maxMoveSpeed.Value;
            _animation.State.Parameter = Mathf.Clamp01(speed / Mathf.Max(maxMoveSpeedValue, 0.001f));
        }

        protected void FixedUpdate() {
            var planarInput = Vector3.ProjectOnPlane(_moveInput.value, motor.Up);
            var maxMoveSpeedValue = maxMoveSpeed.Value;
            var targetVelocity = Vector3.ClampMagnitude(planarInput, 1) * maxMoveSpeedValue;
            var acceleration = moveAcceleration.Value;
            motor.InputVelocity = Vector3.MoveTowards(
                motor.InputVelocity,
                targetVelocity,
                acceleration * Time.fixedDeltaTime);
        }
    }
}
