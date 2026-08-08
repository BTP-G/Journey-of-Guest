using UnityEngine;

namespace Xoderony.Movement {

    public struct GroundDetectionResult {
        public Collider collider;
        public Vector3 point;
        public Vector3 normal;
        public Vector3 velocity;
        public float distance;
        public bool isStable;

    }
}