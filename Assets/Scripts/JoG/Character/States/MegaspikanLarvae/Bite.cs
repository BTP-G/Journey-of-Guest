using JoG.Combat;
using JoG.Health;
using System;
using UnityEngine;
using VContainer;
using Xoderony;

namespace JoG.Character.States.MegaspikanLarvae {

    public class Bite : SkillState {
        public Transform attackPoint;
        public LayerMask attackMask;
        [Min(0)] public float biteRadius;
        public int damageMultiplierPercent = 100;

        public HealthChangeFlag damageFlags;

        [Inject] internal Entity attacker;
        [Inject] internal CombatDamage combatDamage;
        [Inject, Key(Constants.Stats.AttackPower)] internal Stat attackPowerStat;
        private IDelegateSubscriber<AnimationEventHandler> _animationEvents;

        [Inject]
        internal void Inject(IDelegateSubscriber<AnimationEventHandler> animationEvents) {
            _animationEvents = animationEvents;
        }

        protected void OnEnable() {
            animator.SetBool("isBiting", true);
            _animationEvents.Subscribe(HandleAnimationEvent);
        }

        protected void HandleAnimationEvent(in AnimationEvent animationEvent) {
            if (animationEvent.stringParameter == "bite") {
                combatDamage.ApplySphere(
                    attacker,
                    attackPoint.position,
                    biteRadius,
                    attackMask,
                    QueryTriggerInteraction.Ignore,
                    attackPowerStat.Value * damageMultiplierPercent / 100,
                    damageFlags,
                    null,
                    broadcast: true);
            } else if (animationEvent.stringParameter == "exit") {
                //TransitionTo(null);
            }
        }

        protected void OnDisable() {
            animator.SetBool("isBiting", false);
            _animationEvents.Unsubscribe(HandleAnimationEvent);
        }

        private void OnDrawGizmos() {
            Gizmos.color = Color.green;
            attackPoint.GetPositionAndRotation(out var position, out var rotation);
            Gizmos.matrix = Matrix4x4.TRS(position, rotation, Vector3.one);
            Gizmos.DrawWireSphere(Vector3.zero, biteRadius);
            Gizmos.matrix = Matrix4x4.identity;
        }
    }
}
