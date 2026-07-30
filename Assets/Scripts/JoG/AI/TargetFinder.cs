using JoG.AI.Patrol;
using System.Buffers;
using UnityEngine;
using UnityEngine.AI;
using VContainer;

namespace JoG.AI {

    public class TargetFinder : MonoBehaviour, IComponent {
        public LayerMask targetMask;
        public float findRadius = 50f;
        public float findInterval = 1f;
        [Inject] internal AITarget target;
        [Inject] internal Rigidbody body;
        [Inject] internal Faction faction;
        [Inject] internal PatrolService patrolService;
        private PatrolRoute _currentRoute;
        private int _currentRouteIndex;
        private float _lastFindTime;
        private NavMeshAgent _agent;

        [Inject]
        internal void Inject(NavMeshAgentController agentController) {
            _agent = agentController.agent;
        }

        private void Update() {
            if (Time.time - _lastFindTime > findInterval) {
                _lastFindTime = Time.time;
                var colliders = ArrayPool<Collider>.Shared.Rent(256);
                var startPosition = body.position;
                var count = Physics.OverlapSphereNonAlloc(startPosition, findRadius, colliders, targetMask, QueryTriggerInteraction.Ignore);
                var closestSqrDistance = float.MaxValue;
                Collider closestTarget = null;
                for (var i = 0; i < count; i++) {
                    var collider1 = colliders[i];
                    if (collider1.TryGetComponent<HurtBox>(out var reference)
                        && faction.IsHostileTo(reference.Entity.GetComponent<Faction>())) {
                        var sqrDistance = Vector3.SqrMagnitude(startPosition - collider1.transform.position);
                        if (sqrDistance < closestSqrDistance) {
                            closestSqrDistance = sqrDistance;
                            closestTarget = collider1;
                        }
                    }
                }
                ArrayPool<Collider>.Shared.Return(colliders);
                if (closestTarget != null) {
                    target.target = closestTarget.transform;
                    _currentRoute = null;
                } else {
                    if (_currentRoute == null) {
                        _currentRoute = patrolService.GetClosestRoute(startPosition);
                        target.target = _currentRoute.GetClosestPoint(startPosition, out _currentRouteIndex);
                    }
                    if (_agent.remainingDistance <= _agent.stoppingDistance) {
                        target.target = _currentRoute.GetNextPoint(ref _currentRouteIndex);
                    }
                }
            }
        }
    }
}
