using UnityEngine;

namespace Xoderony.Movement {

    public readonly struct SweepResult {
        public readonly Collider collider;
        public readonly Vector3 point;
        public readonly Vector3 normal;
        public readonly float safeDistance;

        public SweepResult(Collider collider, in Vector3 point, in Vector3 normal, float safeDistance) {
            this.collider = collider;
            this.point = point;
            this.normal = normal;
            this.safeDistance = safeDistance;
        }

        public SweepResult(float safeDistance) {
            this.safeDistance = safeDistance;
            this.point = default;
            this.normal = default;
            this.collider = default;
        }
    }
}