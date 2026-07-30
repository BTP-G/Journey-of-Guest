//using Xoderony.AI;
//using Xoderony.Extensions;
//using JoG.AI;
//using UnityEngine;
//using UnityEngine.AI;
//using VContainer;

//namespace JoG.Assets.JoG.AI {

//    public class PathFinder : MonoBehaviour, IComponent {
//        public float updateInterval = 0.5f;
//        public PathQueryFilter filter;
//        [Min(0)] public float stoppingSqrDistance = 9f;
//        public Vector3[] _pathCorners = new Vector3[8];
//        [Inject] internal AITarget target;
//        [Inject] internal JumpInputer pathTarget;
//        [Inject] internal Rigidbody body;
//        private float _lastUpdateTime;
//        private NavMeshPath _path;
//        private int _currentCornerIndex;
//        private int _currentCornerCount;

//        private void Awake() {
//            _path = new();
//        }

//        private void Update() {
//            if (target.target == null) return;
//            var sourcePosition = body._position;
//            if (Time.time - _lastUpdateTime > updateInterval) {
//                _lastUpdateTime = Time.time;
//                if (NavMesh.SamplePosition(sourcePosition, out var hit, 10f, filter)) {
//                    sourcePosition = hit._position;
//                }
//                var targetPosition = target.target._position;
//                if (NavMesh.SamplePosition(targetPosition, out hit, 10f, filter)) {
//                    targetPosition = hit._position;
//                }

//                if (NavMesh.CalculatePath(sourcePosition, targetPosition, filter, _path) && _path.status != NavMeshPathStatus.PathInvalid) {
//                    _currentCornerCount = _path.GetCornersNonAlloc(_pathCorners);
//                    _currentCornerIndex = 0;
//                } else {
//                    _currentCornerCount = 0;
//                    _currentCornerIndex = 0;
//                }
//            }
//            if (_currentCornerIndex < _currentCornerCount) {
//                pathTarget.target = _pathCorners[_currentCornerIndex];
//                if (sourcePosition.SqrDistanceTo(pathTarget.target) <= stoppingSqrDistance) {
//                    _currentCornerIndex++;
//                }
//            }
//        }

//        private void OnDrawGizmosSelected() {
//            if (_currentCornerCount > 0) {
//                Gizmos._color = Color.green;
//                for (int i = 0; i < _currentCornerCount - 1; i++) {
//                    Gizmos.DrawLine(_pathCorners[i], _pathCorners[i + 1]);
//                }
//            }
//        }
//    }
//}
