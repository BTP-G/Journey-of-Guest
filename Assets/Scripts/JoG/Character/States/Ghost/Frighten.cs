using JoG.Health;
using Xoderony;
using Xoderony.PropertyAttributes;
using JoG.Character.Components;
using System;
using System.Buffers;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

namespace JoG.Character.States.Ghost {

    public class Frighten : SkillState {
        public Transform attackPoint;
        public LayerMask attackMask;
        public Vector3 attackSize;
        [Min(0)] public float damageMultiplier = 1;

        [FlagsField(typeof(HealthChangeFlag))]
        public ulong damageFlags;

        [Inject] internal Entity attacker;
        [Inject] internal CharacterAimInputHandler aimInputHandler;
        [Inject, Key(Constants.Stats.AttackPower)] internal Stat attackPowerStat;
        private IDelegateSubscriber<AnimationEventHandler> _animationEvents;
        private readonly HashSet<Entity> hits = new();

        [Inject]
        internal void Inject(IDelegateSubscriber<AnimationEventHandler> animationEvents) {
            _animationEvents = animationEvents;
        }

        protected void OnEnable() {
            _animationEvents.Subscribe(HandleAnimationEvent);
            animator.SetBool(AnimatorHashs.isAttacking, true);
            hits.Clear();
            aimInputHandler.aimTime = float.MaxValue;
        }

        protected void OnDisable() {
            _animationEvents.Unsubscribe(HandleAnimationEvent);
            animator.SetBool(AnimatorHashs.isAttacking, false);
            hits.Clear();
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
                var buffer = ArrayPool<Collider>.Shared.Rent(256);
                var count = Physics.OverlapBoxNonAlloc(attackPoint.position, attackSize * 0.5f, buffer, attackPoint.rotation, attackMask);
                var damageValue = (int)(attackPowerStat.Value * damageMultiplier);
                foreach (var c in buffer.AsSpan(0, count)) {
                    if (c.TryGetComponent<Damageable>(out var damageable) && hits.Add(damageable.Entity) && damageable.CanTakeDamage(attacker)) {
                        var message = new HealthChangeMessage() {
                            Position = c.ClosestPoint(attackPoint.position),
                            //impulse = new Vector3(0, 10, 0),
                            Value = damageValue,
                            Flags = 0,
                        };
                        damageable.TakeDamage(ref message, attacker);
                    }
                }
                ArrayPool<Collider>.Shared.Return(buffer);
            } else if (animationEvent.stringParameter == "exit") {
                //TransitionTo(null);
            }
        }
    }
}
