using System;
using System.Buffers;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Xoderony.Extensions;
using Xoderony.Unity;

namespace Xoderony.Movement {

    [RequireComponent(typeof(Rigidbody))]
    public class CharacterMotor : MonoBehaviour {

        [SerializeField]
        private CapsuleCollider _capsule;

        [SerializeField]
        private LayerMask _stableGroundMask;

        [Range(0f, 89f)]
        [SerializeField]
        private float _maxStableSlopeAngle = 60f;

        private float _minStableDot;

        [Min(0f)]
        [SerializeField]
        private float _maxStepHeight = 0.3f;

        private int _forceUngroundedFrameCount;
        private float _radius;
        private float _contactOffset;
        private bool _isGrounded;
        private bool _isStableGrounded;
        private Rigidbody _rigidbody;
        private Vector3 _groundNormal = Vector3.up;
        private LayerMask _collisionMask;
        private LayerMask _overlapMask;
        private Vector3 _toLowerCenter;
        private Vector3 _toUpperCenter;
        private Vector3 _inputVelocity;
        private Vector3 _externalVelocity;
        private Vector3 _groundVelocity;
        private CapsuleFloatScope _capsuleFloatScope;
        private Vector3 _lastPostition;
        public float MaxStepHeight => _maxStepHeight;

        public Vector3 Position {
            get => _rigidbody.position;
            set => _rigidbody.position = value;
        }

        public Quaternion Rotation {
            get => _rigidbody.rotation;
            set => _rigidbody.rotation = value;
        }

        public bool IsGrounded => _isGrounded;

        public bool IsStable => _isStableGrounded;

        public Vector3 Up => _rigidbody.rotation * Vector3.up;
        public Vector3 InputVelocity { get => _inputVelocity; set => _inputVelocity = value; }
        public Vector3 ExternalVelocity { get => _externalVelocity; set => _externalVelocity = value; }
        public Vector3 Velocity => _inputVelocity + _externalVelocity;
        public Vector3 LocalVelocity => transform.InverseTransformDirection(_inputVelocity + _externalVelocity);

        public void AddImpulse(in Vector3 impulse) {
            _externalVelocity += impulse / _rigidbody.mass;
        }

        public void MoveRotation(in Quaternion rotation) {
            _rigidbody.MoveRotation(rotation);
        }

        public void ForceUngrounded(int frameCount) {
            _forceUngroundedFrameCount = frameCount;
            _isStableGrounded = false;
            _isGrounded = false;
            _groundVelocity = Vector3.zero;
        }

        private void Awake() {
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.maxLinearVelocity = 50f;
            _capsuleFloatScope = new(_capsule);
            _collisionMask = _capsule.GetCollisionLayerMask();
            _overlapMask = _collisionMask & ~(1 << _capsule.gameObject.layer);
            _minStableDot = Mathf.Cos(_maxStableSlopeAngle * Mathf.Deg2Rad);
        }

        private void OnEnable() {
            PostUpdateLoop<FixedUpdate.ClearLines>.Register(PostFixedUpdateClearLines);
            PostUpdateLoop<FixedUpdate.ScriptRunBehaviourFixedUpdate>.Register(PostFixedUpdate);
        }

        private void PostFixedUpdateClearLines() {
            if (_lastPostition.DistanceTo(_rigidbody.position) > 999) {
                Debug.LogError($"super speed: {Velocity} | {_rigidbody.linearVelocity}", this);
            }
            _lastPostition = _rigidbody.position;
            _capsuleFloatScope.Reset();
            UpdateCapsuleGeometry();
            var result = default(GroundDetectionResult);
            _isGrounded = _forceUngroundedFrameCount-- <= 0 && DetectGround(out result);
            _isStableGrounded = result.isStable;
            _groundNormal = result.normal;
            _groundVelocity = result.velocity;
        }

        private void PostFixedUpdate() {
            var dt = Time.fixedDeltaTime;
            (Velocity * dt).GetDirectionAndMagnitude(out var direction, out var distance);
            var startPosition = _rigidbody.position;
            Vector3 targetPosition;
            if (_isStableGrounded) {
                targetPosition = Move(startPosition, direction, distance);
                _externalVelocity = _groundVelocity;
            } else {
                targetPosition = SimpleMove(startPosition, direction, distance);
                _externalVelocity += Physics.gravity * dt;
                _externalVelocity -= 0.001f * dt * _externalVelocity;
            }
            _rigidbody.linearVelocity = (targetPosition - _rigidbody.position) / dt;
        }

        private void OnDisable() {
            PostUpdateLoop<FixedUpdate.ClearLines>.Unregister(PostFixedUpdateClearLines);
            PostUpdateLoop<FixedUpdate.ScriptRunBehaviourFixedUpdate>.Unregister(PostFixedUpdate);
        }

        private void OnValidate() {
            if (_capsule != null) {
                _maxStepHeight = Mathf.Clamp(_maxStepHeight, 0, _capsule.height * 0.5f);
            } else {
                _capsule = GetComponentInChildren<CapsuleCollider>();
            }
        }

        private void UpdateCapsuleGeometry() {
            _contactOffset = _capsule.contactOffset;
            _radius = _capsule.radius;
            var height = _capsule.height;
            var center = _capsule.center;
            var halfCylinderHeight = Mathf.Max(0, (height * 0.5f) - _radius);
            var rotation = _rigidbody.rotation;
            _toLowerCenter = rotation * new Vector3(center.x, center.y - halfCylinderHeight, center.z);
            _toUpperCenter = rotation * new Vector3(center.x, center.y + halfCylinderHeight, center.z);
        }

        private void ResolveOverlap() {
            var direction = Vector3.zero;
            var distance = 0f;
            var positionA = _rigidbody.position;
            var rotationA = _rigidbody.rotation;
            var overlaps = ArrayPool<Collider>.Shared.Rent(256);
            var count = CapsuleOverlap(positionA, overlaps);
            foreach (var other in new ReadOnlySpan<Collider>(overlaps, 0, count)) {
                other.transform.GetPositionAndRotation(out var positionB, out var rotationB);
                if (Physics.ComputePenetration(
                _capsule,
                positionA,
                rotationA,
                other,
                positionB,
                rotationB,
                out var dir,
                out var dis)) {
                    if (dis > distance) {
                        direction = dir;
                        distance = dis;
                    }
                }
            }
            _rigidbody.position = positionA + (direction * distance);
            ArrayPool<Collider>.Shared.Return(overlaps, false);
        }

        private void AdjustDisplacementByOverlaps(ref Vector3 displacement) {
            var positionA = _rigidbody.position;
            var rotationA = _rigidbody.rotation;
            var overlaps = ArrayPool<Collider>.Shared.Rent(256);
            var count = CapsuleOverlap(positionA, overlaps);
            foreach (var other in new ReadOnlySpan<Collider>(overlaps, 0, count)) {
                other.transform.GetPositionAndRotation(out var positionB, out var rotationB);
                if (Physics.ComputePenetration(
                _capsule,
                positionA,
                rotationA,
                other,
                positionB,
                rotationB,
                out var direction,
                out _)) {
                    displacement = displacement.ProjectOnPlane(direction);
                }
            }
            ArrayPool<Collider>.Shared.Return(overlaps, false);
        }

        private int CapsuleOverlap(in Vector3 position, Collider[] overlaps) {
            var lowerCenter = position + _toLowerCenter;
            var upperCenter = position + _toUpperCenter;
            return Physics.OverlapCapsuleNonAlloc(lowerCenter, upperCenter, _radius, overlaps, _overlapMask, QueryTriggerInteraction.Ignore);
        }

        private bool CapsuleSweep(in Vector3 position, in Vector3 direction, float distance, out SweepResult result) {
            var lowerCenter = position + _toLowerCenter;
            var upperCenter = position + _toUpperCenter;
            var isHit = Physics.CapsuleCast(lowerCenter, upperCenter, _radius, direction, out var hit, distance + _contactOffset, _collisionMask, QueryTriggerInteraction.Ignore);
            if (isHit) {
                var dot = direction.Dot(hit.normal);
                hit.distance = Mathf.Max(0, hit.distance + (_contactOffset / dot));
                result = new SweepResult(hit.collider, hit.point, hit.normal, hit.distance);
            } else {
                result = new SweepResult(distance);
            }
            return isHit;
        }

        private bool DetectGround(out GroundDetectionResult result) {
            var upperCenter = _rigidbody.position + _toUpperCenter;
            var direction = _rigidbody.rotation * Vector3.down;
            var up = -direction;
            var cylinderHeight = _capsule.height - (2 * _radius);
            var maxDistance = _isStableGrounded ? (cylinderHeight + _maxStepHeight) : cylinderHeight + (_radius * 0.1f);
            var radius = _radius * 0.99f;
            if (Physics.SphereCast(upperCenter, radius, direction, out var hit0, maxDistance, _collisionMask, QueryTriggerInteraction.Ignore)) {
                result = new() {
                    collider = hit0.collider,
                    point = hit0.point,
                    normal = hit0.normal,
                    distance = hit0.distance - cylinderHeight,
                    isStable = false,
                };
                var groundRigidbody = result.collider.attachedRigidbody;
                if (groundRigidbody != null) {
                    result.velocity = groundRigidbody.GetPointVelocity(result.point);
                }
                if (IsStableGround(result.collider, hit0.normal, up)) {
                    result.normal = hit0.normal;
                    result.isStable = true;
                    return true;
                }
                var origin2 = upperCenter + (direction * hit0.distance);
                var direction2 = direction.ProjectOnPlane(hit0.normal);
                if (Physics.SphereCast(origin2, radius, direction2, out var hit1, _radius * 0.1f, _collisionMask, QueryTriggerInteraction.Ignore)) {
                    var 合并法线 = (hit0.normal + hit1.normal).normalized;
                    if (IsStableGround(result.collider, 合并法线, up)) {
                        result.normal = 合并法线;
                        result.isStable = true;
                        return true;
                    }
                }
                if (Physics.Raycast(upperCenter, direction, out var hit2, maxDistance, _collisionMask, QueryTriggerInteraction.Ignore)) {
                    if (IsStableGround(result.collider, hit2.normal, up)) {
                        result.normal = hit2.normal;
                        result.isStable = true;
                        return true;
                    }
                }
                return true;
            }
            result = default;
            return false;
        }

        private bool IsStableGround(Collider ground, in Vector3 normal, in Vector3 up) {
            if ((_stableGroundMask & (1 << ground.gameObject.layer)) == 0) {
                return false;
            }
            return normal.Dot(up) > _minStableDot;
        }

        private Vector3 Move(in Vector3 startPosition, in Vector3 direction, float distance) {
            _capsuleFloatScope.Float(_maxStepHeight);
            UpdateCapsuleGeometry();
            var targetPosition = startPosition;
            var up = _rigidbody.rotation * Vector3.up;
            if (SimulateMove(ref targetPosition, direction.ProjectOnPlaneAlongDirection(_groundNormal, -up).normalized, ref distance, out var hit)) {
                if (IsStableGround(hit.collider, hit.normal, up)) {
                    SimulateMove(ref targetPosition, direction.ProjectOnPlane(hit.normal).normalized, ref distance, out _);
                }
                //else if (IsHitStair(hit.point, targetPosition + _toLowerCenter - (up * _radius), up)
                //    && SimulateClimbStep(ref targetPosition, direction, ref distance, up)) {
                //}
                else {
                    if (SimulateMove(ref targetPosition, direction.ProjectOnIntersection(hit.normal, _groundNormal), ref distance, out var hit2)) {
                        SimulateMove(ref targetPosition, direction.ProjectOnIntersection(hit.normal, hit2.normal), ref distance, out _);
                    }
                }
            }
            _capsuleFloatScope.Reset();
            UpdateCapsuleGeometry();
            SimulateStepDown(ref targetPosition, up);
            _capsuleFloatScope.Float(_maxStepHeight);
            return targetPosition;
        }

        private Vector3 SimpleMove(in Vector3 startPosition, in Vector3 direction, float distance) {
            var targetPosition = startPosition;
            if (SimulateMove(ref targetPosition, direction, ref distance, out var hit)) {
                if (SimulateMove(ref targetPosition, direction.ProjectOnPlane(hit.normal), ref distance, out var hit2)) {
                    SimulateMove(ref targetPosition, direction.ProjectOnIntersection(hit.normal, hit2.normal), ref distance, out _);
                }
            }
            return targetPosition;
        }

        private bool IsHitStair(in Vector3 hitPoint, in Vector3 capsuleBottom, in Vector3 up) {
            var pointHeightFromCapsuleBottom = up.Dot(hitPoint - capsuleBottom);
            return pointHeightFromCapsuleBottom < _maxStepHeight;
        }

        private bool SimulateMove(ref Vector3 position, in Vector3 direction, ref float distance, out SweepResult result) {
            var isHit = CapsuleSweep(position, direction, distance, out result);
            position += direction * result.safeDistance;
            distance -= result.safeDistance;
            return isHit;
        }

        private bool SimulateClimbStep(ref Vector3 position, in Vector3 direction, ref float distance, in Vector3 up) {
            var startPosition = position;
            var startDistance = distance;
            CapsuleSweep(position, up, _maxStepHeight, out var result);
            if (result.safeDistance < _contactOffset) {
                return false;
            }
            position += up * result.safeDistance;
            CapsuleSweep(position, direction, distance, out result);
            if (result.safeDistance < _contactOffset) {
                position = startPosition;
                return false;
            }
            position += direction * result.safeDistance;
            distance -= result.safeDistance;
            var down = -up;
            CapsuleSweep(position, down, _maxStepHeight, out result);
            if (!IsStableGround(result.collider, result.normal, up)) {
                position = startPosition;
                distance = startDistance;
                return false;
            }
            position += down * result.safeDistance;
            down = down.ProjectOnPlane(result.normal);
            CapsuleSweep(position, down, _maxStepHeight - result.safeDistance, out result);
            if (!IsStableGround(result.collider, result.normal, up)) {
                position = startPosition;
                distance = startDistance;
                return false;
            }
            position += down * result.safeDistance;
            return true;
        }

        private void SimulateStepDown(ref Vector3 position, in Vector3 up) {
            position += up * _maxStepHeight;
            var direction = -up;
            var distance = _maxStepHeight * 2;
            if (CapsuleSweep(position, direction, distance, out var result)) {
                position += direction * result.safeDistance;
                if (!IsStableGround(result.collider, result.normal, up)) {
                    direction = direction.ProjectOnPlane(result.normal);
                    distance -= result.safeDistance;
                    CapsuleSweep(position, direction, distance, out result);
                    position += direction * result.safeDistance;
                }
            }
        }
    }
}