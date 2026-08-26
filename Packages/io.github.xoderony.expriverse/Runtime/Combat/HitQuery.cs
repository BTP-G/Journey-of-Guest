using System;
using System.Buffers;
using System.Collections.Generic;
using Expriverse.Health;
using UnityEngine;
using Xoderony.Extensions;
using Xoderony.ObjectPool.Generic;

namespace Expriverse.Combat {

    /// <summary>一次性形状查询：重叠结果按实体去重，仅保留距离源点最近的碰撞体。</summary>
    public static class HitQuery {

        private const int BufferCapacity = 256;

        public static void CollectSphere(in Vector3 position, float radius, LayerMask mask, QueryTriggerInteraction interaction, List<HitResult> results) {
            results.Clear();
            var buffer = ArrayPool<Collider>.Shared.Rent(BufferCapacity);
            try {
                var count = Physics.OverlapSphereNonAlloc(position, radius, buffer, mask, interaction);
                Collect(position, buffer, count, results);
            } finally {
                ArrayPool<Collider>.Shared.Return(buffer);
            }
        }

        public static void CollectBox(in Vector3 position, in Vector3 size, in Quaternion rotation, LayerMask mask, QueryTriggerInteraction interaction, List<HitResult> results) {
            results.Clear();
            var buffer = ArrayPool<Collider>.Shared.Rent(BufferCapacity);
            try {
                var count = Physics.OverlapBoxNonAlloc(position, size * 0.5f, buffer, rotation, mask, interaction);
                Collect(position, buffer, count, results);
            } finally {
                ArrayPool<Collider>.Shared.Return(buffer);
            }
        }

        private static void Collect(in Vector3 origin, Collider[] buffer, int count, List<HitResult> results) {
            using (DictionaryPool<Entity, HitResult>.Rent(out var entityToHit)) {
                foreach (var collider in buffer.AsSpan(0, count)) {
                    collider.TryGetComponent<Damageable>(out var damageable);
                    collider.TryGetComponent<Healable>(out var healable);
                    if (damageable == null && healable == null) {
                        continue;
                    }
                    var entity = damageable != null ? damageable.Entity : healable.Entity;
                    var point = collider.ClosestPoint(origin);
                    var sqrDistance = point.SqrDistanceTo(origin);
                    if (entityToHit.TryGetValue(entity, out var existing) && sqrDistance >= existing.SqrDistance) {
                        continue;
                    }
                    entityToHit[entity] = new HitResult(entity, collider, point, sqrDistance);
                }
                foreach (var hit in entityToHit.Values) {
                    results.Add(hit);
                }
            }
        }
    }
}
