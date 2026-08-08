using Animancer;
using UnityEngine;
using VContainer;
using Xoderony.Movement;

namespace JoG.Character.States {

    public class CharacterIdleState : CharacterAnimancerState {

        [SerializeField]
        private ClipTransition _animation = new();

        [Inject]
        internal CharacterMotor motor;

        [Inject]
        [Key(Constants.Stats.MoveAcceleration)]
        internal Stat moveAcceleration;

        protected void OnEnable() {
            Play(_animation);
        }

        protected void FixedUpdate() {
            var acceleration = moveAcceleration.Value;
            motor.InputVelocity = Vector3.MoveTowards(motor.InputVelocity, Vector3.zero, acceleration * Time.fixedDeltaTime);
        }
    }
}
