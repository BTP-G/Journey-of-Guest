using UnityEngine;

namespace JoG.Projectiles {

    public struct ProjectileHitMessage {
        public Collider collider;

        public Rigidbody rigidbody;

        public Vector3 position;

        public Vector3 normal;

        public ProjectileHitMessage(RaycastHit hit) {
            collider = hit.collider;
            rigidbody = collider.attachedRigidbody;
            position = hit.point;
            normal = hit.normal;
        }

        public ProjectileHitMessage(ContactPoint contactPoint) {
            collider = contactPoint.otherCollider;
            rigidbody = collider.attachedRigidbody;
            position = contactPoint.point;
            normal = contactPoint.normal;
        }
    }
}