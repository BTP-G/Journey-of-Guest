using Xoderony.Movement;
using EditorAttributes;
using JoG.Character.InputBanks;
using JoG.StateMachines;
using UnityEngine;
using VContainer;

namespace JoG.Character.States {

    public class CharacterLocomotionStateMachine : MonoStateMachine, IComponent {
        [SerializeField, Required]
        private CharacterIdleState _idleState;

        [SerializeField, Required]
        private CharacterMoveState _moveState;

        [SerializeField, Required]
        private CharacterAirState _airState;

        [SerializeField, Min(0)]
        private float _moveDeadZone = 0.01f;

        private float _moveDeadZoneSquared;

        [Inject]
        internal CharacterMotor motor;

        private MoveInputBank _moveInput;

        [Inject]
        internal void Inject(InputBankHub inputBankHub) {
            _moveInput = inputBankHub.GetInputBank<MoveInputBank>();
        }

        protected void Awake() {
            _moveDeadZoneSquared = _moveDeadZone * _moveDeadZone;
            _idleState.Exit();
            _moveState.Exit();
            _airState.Exit();
        }

        protected override void OnEnable() {
            base.OnEnable();
            TransitionTo(GetNextState());
        }

        protected override void OnDisable() {
            base.OnDisable();
            motor.ExternalVelocity = motor.InputVelocity;
            motor.InputVelocity = Vector3.zero;
        }

        protected void Update() {
            TransitionTo(GetNextState());
        }

        private CharacterAnimancerState GetNextState() {
            if (!motor.IsStable) {
                return _airState;
            }

            var planarInput = Vector3.ProjectOnPlane(_moveInput.vector3, motor.Up);
            return planarInput.sqrMagnitude > _moveDeadZoneSquared ? _moveState : _idleState;
        }
    }

}
