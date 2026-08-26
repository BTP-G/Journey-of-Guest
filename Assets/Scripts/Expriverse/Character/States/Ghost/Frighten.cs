using Expriverse.Character.Components;
using Expriverse.Combat;
using Expriverse.Health;
using System;
using UnityEngine;
using VContainer;
using Xoderony;
using Xoderony.PropertyAttributes;

namespace Expriverse.Character.States.Ghost {

    public class Frighten : SkillState {
        public Transform attackPoint;
        public LayerMask attackMask;
        public Vector3 attackSize;
        [Min(0)] public float damageMultiplier = 1;

        [FlagsField(typeof(HealthChangeFlag))]
        public ulong damageFlags;

        [Inject] internal Entity attacker;
        [Inject] internal CombatDamage combatDamage;
        [Inject] internal CharacterAimInputHandler aimInputHandler;
        [Inject, Key(Constants.Stats.AttackPower)] internal Stat attackPowerStat;
        private IDelegateSubscriber<AnimationEventHandler> _animationEvents;

        [Inject]
        internal void Inject(IDelegateSubscriber<AnimationEventHandler> animationEvents) {
            _animationEvents = animationEvents;
        }

        protected void OnEnable() {
            _animationEvents.Subscribe(HandleAnimationEvent);
            animator.SetBool(AnimatorHashs.isAttacking, true);
            aimInputHandler.aimTime = float.MaxValue;
        }

        protected void OnDisable() {
            _animationEvents.Unsubscribe(HandleAnimationEvent);
            animator.SetBool(AnimatorHashs.isAttacking, false);
            aimInputHandler.aimTime = 1;
        }

        protected void OnDrawGizmos() {
            Gizmos.color = Color.green;
            attackPoint.GetPositionAndRotation(out var position, out var rotation);
            Gizmos.matrix = Matrix4x4.TRS(position, rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, attackSize);
            Gizmos.matrix = Matrix4x4.identity;
        }

        private void HandleAnimationEvent(in AnimationEvent animationEvent) {
            if (animationEvent.stringParameter == "frighten") {
                combatDamage.ApplyBox(
                    attacker,
                    attackPoint.position,
                    attackSize,
                    attackPoint.rotation,
                    attackMask,
                    QueryTriggerInteraction.Collide,
                    attackPowerStat.Value * damageMultiplier,
                    (HealthChangeFlag)damageFlags,
                    broadcast: true);
            } else if (animationEvent.stringParameter == "exit") {
                //TransitionTo(null);
            }
        }
    }
}
