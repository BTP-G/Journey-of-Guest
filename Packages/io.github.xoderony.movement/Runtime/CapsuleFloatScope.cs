using UnityEngine;

namespace Xoderony.Movement {

    internal readonly struct CapsuleFloatScope {
        public readonly CapsuleCollider capsule;
        public readonly Vector3 orignalCenter;
        public readonly float orignalHeight;

        public CapsuleFloatScope(CapsuleCollider capsule) {
            this.capsule = capsule;
            orignalCenter = capsule.center;
            orignalHeight = capsule.height;
        }

        public void Float(float height) {
            capsule.height = orignalHeight - height;
            capsule.center = orignalCenter + new Vector3(0, height * 0.5f, 0);
        }

        public void Reset() {
            capsule.center = orignalCenter;
            capsule.height = orignalHeight;
        }
    }
}