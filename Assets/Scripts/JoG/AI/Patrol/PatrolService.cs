using Xoderony.Extensions;
using System;
using UnityEngine;

namespace JoG.AI.Patrol {

    [Serializable]
    public class PatrolService {
        public PatrolRoute[] routes = Array.Empty<PatrolRoute>();

        public PatrolRoute GetClosestRoute(in Vector3 current) {
            PatrolRoute closestRoute = null;
            var currentSqrDistance = float.MaxValue;
            foreach (var route in routes) {
                var sqrDistance = current.SqrDistanceTo(route.centerPoint.position);
                if (sqrDistance < currentSqrDistance) {
                    closestRoute = route;
                    currentSqrDistance = sqrDistance;
                }
            }
            return closestRoute;
        }
    }
}
