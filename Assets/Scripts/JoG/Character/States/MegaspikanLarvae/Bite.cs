using JoG.Health;
using System;
using System.Buffers;
using System.Collections.Generic;
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
        [Inject, Key(Constants.Stats.AttackPower)] internal Stat attackPowerStat;
        private HashSet<Entity> hits = new();
        private IDelegateSubscriber<AnimationEventHandler> _animationEvents;

        [Inject]
        internal void Inject(IDelegateSubscriber<AnimationEventHandler> animationEvents) {
            _animationEvents = animationEvents;
        }

        protected void OnEnable() {
            animator.SetBool("isBiting", true);
            _animationEvents.Subscribe(HandleAnimationEvent);
            hits.Clear();
        }

        protected void HandleAnimationEvent(in AnimationEvent animationEvent) {
            if (animationEvent.stringParameter == "bite") {
                var buffer = ArrayPool<Collider>.Shared.Rent(256);
                var count = Physics.OverlapSphereNonAlloc(attackPoint.position, biteRadius, buffer, attackMask, QueryTriggerInteraction.Ignore);
                var damageValue = attackPowerStat.Value * damageMultiplierPercent / 100;
                foreach (var c in buffer.AsSpan(0, count)) {
                    if (c.TryGetComponent<Damageable>(out var damageable)
                        && damageable.Entity != attacker
                        && hits.Add(damageable.Entity)
                        && damageable.CanTakeDamage(attacker)) {
                        var message = new HealthChangeMessage() {
                            Position = c.ClosestPoint(attackPoint.position),
                            //impulse = new Vector3(0, 10, 0),
                            Value = damageValue,
                            Flags = damageFlags,
                        };
                        damageable.TakeDamage(ref message, attacker);
                    }
                }
                ArrayPool<Collider>.Shared.Return(buffer);
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
