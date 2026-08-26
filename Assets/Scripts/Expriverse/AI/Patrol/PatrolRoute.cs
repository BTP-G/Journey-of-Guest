using System;
using UnityEngine;
using Xoderony.Extensions;

namespace Expriverse.AI.Patrol {

    [Serializable]
    public class PatrolRoute {
        public Transform centerPoint;
        public Transform[] points = Array.Empty<Transform>();

        public Transform GetClosestPoint(in Vector3 current, out int index) {
            var currentSqrDistance = float.MaxValue;
            Transform closestPoint = null;
            for (var i = index = 0; i < points.Length; ++i) {
                var point = points[i];
                var sqrDistance = current.SqrDistanceTo(point.position);
                if (sqrDistance < currentSqrDistance) {
                    index = i;
                    closestPoint = point;
                    currentSqrDistance = sqrDistance;
                }
            }
            return closestPoint;
        }

        public Transform GetNextPoint(ref int index) {
            index = (index + 1) % points.Length;
            return points[index];
        }
    }
}
