using UnityEngine;

namespace JoG.TransformExtensions {

    public static class TransformExtensions {

        /// <summary>返回父物体相对与自身的局部位置偏移和局部旋转偏移</summary>
        /// <param name="child"></param>
        /// <param name="positionOffset"></param>
        /// <param name="rotationOffset"></param>
        public static void GetParentPositionAndRotationOffset(this Transform child, out Vector3 positionOffset, out Quaternion rotationOffset) {
            positionOffset = child.InverseTransformDirection(child.parent.position - child.position);
            rotationOffset = Quaternion.Inverse(child.localRotation);
        }

        public static void SetParentParent(this Transform child, Transform parentParent) {
            var positionOffset = child.InverseTransformDirection(child.parent.position - child.position);
            var rotationOffset = Quaternion.Inverse(child.localRotation);
            var parent = child.parent;
            parentParent.GetPositionAndRotation(out var targetPosition, out var targetRotation);
            parent.SetPositionAndRotation(targetPosition + parentParent.TransformDirection(positionOffset), targetRotation * rotationOffset);
            parent.SetParent(parentParent);
        }
    }
}