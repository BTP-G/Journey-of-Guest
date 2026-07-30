using UnityEngine;

namespace Xoderony.Extensions {

    public static class CapsuleExtensions {

        public static void GetLowerAndUpperCenterOffset(this CapsuleCollider capsule, out Vector3 toLowerCenter, out Vector3 toUpperCenter) {
            var rotation = capsule.transform.rotation;
            var radius = capsule.radius;
            var height = capsule.height;
            var center = capsule.center;
            var halfCylinderHeight = Mathf.Max(0, height * 0.5f - radius);
            toLowerCenter = rotation * new Vector3(center.x, center.y - halfCylinderHeight, center.z);
            toUpperCenter = rotation * new Vector3(center.x, center.y + halfCylinderHeight, center.z);
        }
    }
}