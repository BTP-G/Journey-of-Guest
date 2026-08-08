using JoG.Core;
using JoG.Health;
using System;
using System.Buffers;
using UnityEngine;
using VContainer;
using Xoderony.Extensions;
using Xoderony.ObjectPool.Generic;

namespace JoG.Projectiles {

    public class ProjectileExplosion : MonoBehaviour, IComponent, INetworkDespawnHandler {
        [Min(0)] public float explosionRadius = 1;
        [Min(0)] public float explosionForce = 1;

        public HealthChangeFlag damageFlags;

        public LayerMask hitLayer;
        public AnimationCurve falloffCurve;
        public ProjectileDamageEvent onDamage = new();

        private PropertyValue<float> _damageValue;
        private PropertyValue<Entity> _attacker;

        public void Detonate(in Vector3 position) {
            var buffer = ArrayPool<Collider>.Shared.Rent(256);
            var count = Physics.OverlapSphereNonAlloc(position, explosionRadius, buffer, hitLayer, QueryTriggerInteraction.Collide);
            using (DictionaryPool<Entity, ExplosionHit>.Rent(out var entityToHit)) {
                foreach (var collider in buffer.AsSpan(0, count)) {
                    if (!collider.TryGetComponent<Damageable>(out var damageable)) {
                        continue;
                    }
                    if (!damageable.CanTakeDamage(_attacker.value)) {
                        continue;
                    }
                    var victim = damageable.Entity;
                    var hitPoint = collider.ClosestPoint(position);
                    var sqrDistance = hitPoint.SqrDistanceTo(position);
                    if (entityToHit.TryGetValue(victim, out var existing) && sqrDistance >= existing.sqrDistance) {
                        continue;
                    }
                    entityToHit[victim] = new ExplosionHit {
                        collider = collider,
                        damageable = damageable,
                        point = hitPoint,
                        sqrDistance = sqrDistance,
                    };
                }
                foreach (var hit in entityToHit.Values) {
                    var distance = Mathf.Sqrt(hit.sqrDistance);
                    var falloff = falloffCurve.Evaluate(distance / explosionRadius);
                    var direction = (hit.collider.attachedRigidbody.worldCenterOfMass - position).normalized;
                    var message = new HealthChangeMessage {
                        Value = (int)(_damageValue.value * falloff),
                        Flags = damageFlags,
                        Position = hit.point,
                        //impulse = explosionForce * falloff * direction,
                    };
                    hit.damageable.TakeDamage(ref message, _attacker.value);
                    onDamage.Invoke(_attacker.value, hit.damageable.Entity, message);
                }
            }
            ArrayPool<Collider>.Shared.Return(buffer);
        }

        void INetworkDespawnHandler.OnDespawn(bool isOwner) {
        }

        [Inject]
        internal void Inject(PropertyHub propertyHub) {
            _damageValue = propertyHub.GetProperty<float>(Properties.DamageValue);
            _attacker = propertyHub.GetProperty<Entity>(Properties.Attacker);
        }

        protected void Reset() {
            hitLayer = LayerMasks.CharacterPart | LayerMasks.Prop;
            falloffCurve = AnimationCurve.Linear(0, 1, 1, 0);
        }

        protected void OnDrawGizmosSelected() {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }

        private struct ExplosionHit {
            public Collider collider;
            public Damageable damageable;
            public Vector3 point;
            public float sqrDistance;
        }
    }
}
