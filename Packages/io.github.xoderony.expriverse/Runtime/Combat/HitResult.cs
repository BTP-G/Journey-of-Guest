using UnityEngine;

namespace Expriverse.Combat {

    /// <summary>一次形状查询命中的目标；同一实体只保留距离源点最近的碰撞体。能力组件由调用方从 Collider 解析。</summary>
    public readonly struct HitResult {

        public readonly Entity Entity;

        public readonly Collider Collider;

        public readonly Vector3 Point;

        public readonly float SqrDistance;

        public HitResult(Entity entity, Collider collider, in Vector3 point, float sqrDistance) {
            Entity = entity;
            Collider = collider;
            Point = point;
            SqrDistance = sqrDistance;
        }
    }
}
