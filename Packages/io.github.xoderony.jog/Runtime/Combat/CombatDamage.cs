using System.Collections.Generic;
using JoG.Health;
using UnityEngine;
using UnityEngine.Assertions;
using VContainer;
using Xoderony.ObjectPool.Generic;

namespace JoG.Combat {

    /// <summary>统一的伤害施加入口：形状查询、阵营准入、消息构造与本地/广播路由。</summary>
    public sealed class CombatDamage {

        [Inject]
        internal HealthChangeRouter healthChangeRouter;

        /// <param name="damage">正的伤害量；模块内部取负作为 HealthChangeMessage.Value。</param>
        public void ApplySphere(Entity source, in Vector3 position, float radius, LayerMask mask, QueryTriggerInteraction interaction, float damage, HealthChangeFlag flags, AnimationCurve falloff, bool broadcast) {
            using (ListPool<HitResult>.Rent(out var hits)) {
                HitQuery.CollectSphere(position, radius, mask, interaction, hits);
                Apply(source, hits, damage, flags, falloff, radius, broadcast);
            }
        }

        /// <param name="damage">正的伤害量；模块内部取负作为 HealthChangeMessage.Value。</param>
        public void ApplyBox(Entity source, in Vector3 position, in Vector3 size, in Quaternion rotation, LayerMask mask, QueryTriggerInteraction interaction, float damage, HealthChangeFlag flags, bool broadcast) {
            using (ListPool<HitResult>.Rent(out var hits)) {
                HitQuery.CollectBox(position, size, rotation, mask, interaction, hits);
                Apply(source, hits, damage, flags, null, 0f, broadcast);
            }
        }

        /// <param name="damage">正的伤害量；模块内部取负作为 HealthChangeMessage.Value。</param>
        public void ApplySingle(Entity source, Collider collider, in Vector3 point, float damage, HealthChangeFlag flags, bool broadcast) {
            Assert.IsTrue(damage >= 0f);
            if (!collider.TryGetComponent<Damageable>(out var damageable) || !damageable.CanTakeDamage(source)) {
                return;
            }
            ApplyDamage(source, damageable.Entity, point, (int)damage, flags, broadcast);
        }

        private void Apply(Entity source, List<HitResult> hits, float damage, HealthChangeFlag flags, AnimationCurve falloff, float falloffRadius, bool broadcast) {
            Assert.IsTrue(damage >= 0f);
            Assert.IsTrue(falloff == null || falloffRadius > 0f);
            foreach (var hit in hits) {
                if (!hit.Collider.TryGetComponent<Damageable>(out var damageable) || !damageable.CanTakeDamage(source)) {
                    continue;
                }
                var amount = falloff != null
                    ? (int)(damage * falloff.Evaluate(Mathf.Sqrt(hit.SqrDistance) / falloffRadius))
                    : (int)damage;
                ApplyDamage(source, hit.Entity, hit.Point, amount, flags, broadcast);
            }
        }

        private void ApplyDamage(Entity source, Entity target, in Vector3 point, int amount, HealthChangeFlag flags, bool broadcast) {
            var message = new HealthChangeMessage {
                Value = -amount,
                Flags = flags,
                Position = point,
            };
            if (broadcast) {
                healthChangeRouter.Broadcast(source, target, ref message);
            } else {
                healthChangeRouter.Route(source, target, ref message);
            }
        }
    }
}
