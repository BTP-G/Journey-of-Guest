using EditorAttributes;
using GuestUnion;
using System;
using System.Buffers;
using UnityEngine;

namespace JoG.Character.Move {

    public struct CharacterGroundStatus {
        public bool IsStableOnGround;
        public bool FoundAnyGround;
        public float DistanceToGround;
        public Vector3 GroundNormal;
        public Vector3 GroundPoint;
        public Collider GroundCollider;
    }

    public partial class RigidbodyCharacterController {
        public const int MaxHitsBudget = 16;
        public const float GroundProbingBackstepDistance = 0.1f;
        public const float MinimumGroundProbingDistance = 0.005f;
        public const float NormalProbeGroundDistance = 0.5f;
        public const float NormalMaxStableSlopeAngle = 70f;
        public const float NormalMaxStepHeight = 0.3f;
        public CharacterGroundStatus groundStatus;
        public CharacterGroundStatus lastGroundStatus;

        [Header("MovementSettings")]
        public ScaledSingle acceleration = 50f;

        [Tooltip("空中加速度 = airControl * acceleration / The air control value of this character as a fraction of ground control.")]
        public float airControl = 0.01f;

        public ScaledSingle maxStableMoveSpeed = 5f;

        [ReadOnly]
        public Vector3 currentVelocity;

        [ReadOnly]
        public Vector3 lastVelocity;

        [Header("OrientationSettings")]
        public bool zTowardsAimDirection;

        [Range(0, 1000)]
        public float forwardOrientationSharpness = 20f;

        public bool yTowardsGravity;

        [Range(0, 1000)]
        public float downOrientationSharpness = 20f;

        [Header("GroundingSettings")]
        [Range(0, 90f)]
        public float maxStableSlopeAngle = NormalMaxStableSlopeAngle;

        [Range(MinimumGroundProbingDistance, float.MaxValue)]
        public float groundProbingDistance = NormalProbeGroundDistance;

        [Header("StepSettings")]
        [Range(0, float.MaxValue)]
        public float maxStepHeight = NormalMaxStepHeight;

        [Header("OtherSettings")]
        public LayerMask collisionMask = Physics.DefaultRaycastLayers;

        public Vector3 gravity = Physics.gravity;

        [NonSerialized]
        public Vector3 moveDirection;

        [NonSerialized]
        public Vector3 aimDirection;

        private static readonly ArrayPool<RaycastHit> arrayPool = ArrayPool<RaycastHit>.Create(MaxHitsBudget, MaxHitsBudget);

        private Vector3 _velocityFromGround;

        private Vector3 _angularVelocityFromGround;

        [SerializeField, Required] private CapsuleCollider _capsule;

        private float currentMustUngroundTime;

        private Quaternion _rotation;

        private RaycastHit[] _hits;

        [SerializeField, Required] private Rigidbody _rigidbody;

        private Optional<Rigidbody> _groundRigidbody;

        private Vector3 _capsuleBottomOffset;

        private Vector3 _capsuleTopOffset;

        private Vector3 _characterForward;

        private Vector3 _characterRight;

        private Vector3 _characterUp;

        private Vector3 _position;

        public float MustUngroundTimeCounter => currentMustUngroundTime;

        public float ProbeGroundDistance { get => groundProbingDistance; set => groundProbingDistance = value > MinimumGroundProbingDistance ? value : MinimumGroundProbingDistance; }

        public Quaternion Rotation => _rotation;

        public Rigidbody Rigidbody => _rigidbody;

        public Vector3 CharacterForward => _characterForward;

        public Vector3 CharacterRight => _characterRight;

        public Vector3 CharacterUp => _characterUp;

        public Vector3 Position => _position;

        public Vector3 VelocityFromGround => _velocityFromGround;

        public Vector3 AngularVelocityFromGround => _angularVelocityFromGround;

        public Optional<Rigidbody> GroundRigidbody => _groundRigidbody;

        public bool IsStableOnGround => groundStatus.IsStableOnGround;

        public float CapsuleHeight {
            get => _capsule.height;
            set {
                _capsule.height = value;
                UpdateCapsuleTopAndBottomOffset();
            }
        }

        public Vector3 CapsuleCenter {
            get => _capsule.center;
            set {
                _capsule.center = value;
                UpdateCapsuleTopAndBottomOffset();
            }
        }

        public float CapsuleRadius {
            get => _capsule.radius;
            set {
                _capsule.radius = value;
                UpdateCapsuleTopAndBottomOffset();
            }
        }

        public event Action<RigidbodyCharacterController> OnHitGround;

        public void UpdateDirection(in Vector3 moveDirection, in Vector3 aimDirection) {
            this.moveDirection = moveDirection;
            this.aimDirection = aimDirection;
        }

        public void UpdateMoveDirection(in Vector2 moveInputVector) {
            var moveVector3 = new Vector3(moveInputVector.x, 0, moveInputVector.y);
            var cameraPlanerDirection = Vector3.ProjectOnPlane(aimDirection, _characterUp).normalized;
            moveDirection = Quaternion.LookRotation(cameraPlanerDirection, _characterUp) * moveVector3;  // 输入方向变换为角色移动方向
        }

        public void ForcedOffTheGround(float time = 0.1f) {
            currentMustUngroundTime = Time.time + time;
        }

        /// <summary>从指定位置向指定方向投射角色胶囊体，返还所有碰撞</summary>
        /// <param _inputName="origin">起始位置</param>
        /// <param _inputName="direction">方向</param>
        /// <param _inputName="hits">碰撞结果</param>
        /// <param _inputName="maxDistance">检测距离</param>
        /// <returns>检测到的碰撞体数量</returns>
        public int CharacterCapsuleCastNonAlloc(in Vector3 origin, in Vector3 direction, float radius, out RaycastHit[] hits, float maxDistance) {
            hits = _hits;
            return InternalCharacterCapsuleCastNonAlloc(origin, direction, radius, maxDistance);
        }

        /// <summary>从指定位置向指定方向投射角色胶囊体，返还第一个碰撞</summary>
        /// <param _inputName="origin">起始位置</param>
        /// <param _inputName="direction">方向</param>
        /// <param _inputName="hitInfo">碰撞结果</param>
        /// <param _inputName="maxDistance">检测距离</param>
        /// <returns>是否发生碰撞</returns>
        public bool CharacterCapsuleCast(in Vector3 origin, in Vector3 direction, out RaycastHit hitInfo, float maxDistance) {
            return Physics.CapsuleCast(origin + (_rotation * _capsuleBottomOffset),
                                       origin + (_rotation * _capsuleTopOffset),
                                       _capsule.radius,
                                       direction,
                                       out hitInfo,
                                       maxDistance,
                                       collisionMask,
                                       QueryTriggerInteraction.Ignore);
        }

        public bool IsStableOnNormal(in Vector3 normal) => Vector3.Angle(_characterUp, normal) <= maxStableSlopeAngle;

        public bool IsValidCollision(in RaycastHit hit) => hit.colliderInstanceID != _capsule.GetInstanceID();

        public void UpdateCapsuleTopAndBottomOffset() {
            _capsuleTopOffset = _capsule.center + ((_capsule.height * 0.5f - _capsule.radius) * Vector3.up);
            _capsuleBottomOffset = _capsule.center + ((-_capsule.height * 0.5f + _capsule.radius) * Vector3.up);
        }

        public Vector3 GetDirectionTangentToSurface(in Vector3 direction, in Vector3 surfaceNormal) => surfaceNormal.Cross(direction.Cross(_characterUp)).normalized;

        public void GetPointVelocityFromRigidbody(Rigidbody rigidbody, in Vector3 point, out Vector3 linearVelocity, out Vector3 angularVelocity) {
            linearVelocity = rigidbody.GetPointVelocity(point);
            angularVelocity = rigidbody.angularVelocity;
        }

        public void UpdateCapsule(in Vector3 center, float height) {
            _capsule.center = center;
            _capsule.height = height;
            UpdateCapsuleTopAndBottomOffset();
        }

        private void UpdateGroundStatus() {
            lastGroundStatus = groundStatus;
            if (Time.time < currentMustUngroundTime) {
                groundStatus = new() { GroundNormal = _characterUp };
                _groundRigidbody = default;
                return;
            }
            ProbeGround(_position + GroundProbingBackstepDistance * _characterUp,
                 -_characterUp,
                 groundProbingDistance + GroundProbingBackstepDistance + (lastGroundStatus.IsStableOnGround ? 2 * maxStepHeight : maxStepHeight),
                 out groundStatus);
            groundStatus.DistanceToGround -= GroundProbingBackstepDistance;
            if (groundStatus.FoundAnyGround) {
                _groundRigidbody = new Optional<Rigidbody>(groundStatus.GroundCollider.attachedRigidbody);
                if (!lastGroundStatus.IsStableOnGround) {
                    OnHitGround?.Invoke(this);
                }
            } else {
                _groundRigidbody = default;
            }
        }

        private int InternalCharacterCapsuleCastNonAlloc(in Vector3 origin, in Vector3 direction, float radius, float maxDistance) {
            return Physics.CapsuleCastNonAlloc(origin + (_rotation * _capsuleBottomOffset),
                                               origin + (_rotation * _capsuleTopOffset),
                                               radius,
                                               direction,
                                               _hits,
                                               maxDistance,
                                               collisionMask,
                                               QueryTriggerInteraction.Ignore);
        }

        private void ProbeGround(in Vector3 origin, in Vector3 direction, float maxDistance, out CharacterGroundStatus groundStatus) {
            var hitsAmount = InternalCharacterCapsuleCastNonAlloc(origin,
                                                                  direction,
                                                                  _capsule.radius * 0.999f,
                                                                  maxDistance);
            groundStatus = new() { GroundNormal = _characterUp };
            if (hitsAmount > 0) {
                var closestHit = new RaycastHit { distance = Mathf.Infinity, };
                var stableHit = closestHit;
                while (--hitsAmount >= 0) {
                    var hit = _hits[hitsAmount];
                    if (hit.distance > 0 && IsValidCollision(hit)) {
                        if (hit.distance < closestHit.distance) {
                            closestHit = hit;
                            groundStatus.FoundAnyGround = true;
                        }
                        if (hit.distance < stableHit.distance && IsStableOnNormal(hit.normal)) {
                            stableHit = hit;
                            groundStatus.IsStableOnGround = true;
                        }
                    }
                }
                if (groundStatus.IsStableOnGround) {
                    groundStatus.GroundNormal = stableHit.normal;
                    groundStatus.GroundCollider = stableHit.collider;
                    groundStatus.GroundPoint = stableHit.point;
                    groundStatus.DistanceToGround = stableHit.distance;
                } else if (groundStatus.FoundAnyGround) {
                    groundStatus.GroundNormal = closestHit.normal;
                    groundStatus.GroundCollider = closestHit.collider;
                    groundStatus.GroundPoint = closestHit.point;
                    groundStatus.DistanceToGround = closestHit.distance;
                    var origin2 = origin + _rotation * _capsuleBottomOffset;
                    var offset = 0.55f * _capsule.radius * _characterForward;
                    var maxDistance2 = maxDistance + _capsule.radius;
                    // 处于悬崖边缘，且边缘可稳定站立
                    if ((Physics.Raycast(origin2 + offset, direction, out var hitInfo, maxDistance2, collisionMask, QueryTriggerInteraction.Ignore)
                            && IsStableOnNormal(hitInfo.normal))
                        || (Physics.Raycast(origin2 - offset, direction, out hitInfo, maxDistance2, collisionMask, QueryTriggerInteraction.Ignore)
                            && IsStableOnNormal(hitInfo.normal))) {
                        groundStatus.IsStableOnGround = true;
                    } else {
                        // 处于V形沟壑中
                        origin2 = origin + (groundStatus.DistanceToGround * direction);
                        if (Physics.CapsuleCast(origin2 + (_rotation * _capsuleTopOffset), origin2 + (_rotation * _capsuleBottomOffset), _capsule.radius, Vector3.ProjectOnPlane(groundStatus.GroundNormal, _characterUp), maxStepHeight, collisionMask, QueryTriggerInteraction.Ignore)) {
                            groundStatus.GroundNormal = _characterUp;  // 累加两个法向量，更加适合移动
                            groundStatus.IsStableOnGround = true;
                        }
                    }
                }
            }
        }

        //private void TryStepUp(in Vector3 direction, float checkDistance = 0.05f) {
        //    var backstepDistance = 0.1f * _capsule.radius;
        //    var origin = _position - backstepDistance * (direction - _characterUp);
        //    checkDistance += backstepDistance;
        //    var closestStepHit = defaultAttacker(RaycastHit);
        //    var continueTry = false;
        //    var groundPosition = groundStatus.GroundPoint;
        //    var hitAmount = CharacterCapsuleCastNonAlloc(origin,
        //                                                 direction,
        //                                                 out var _hits,
        //                                                 checkDistance);
        //    var cachedSqrMagnitude = float.MaxValue;
        //    while (--hitAmount >= 0) {
        //        var _ignoredVictims = _hits[hitAmount];
        //        if (IsValidCollision(_ignoredVictims)
        //            && !IsStableOnNormal(_ignoredVictims.normal)
        //            && (_ignoredVictims.point - groundPosition).sqrMagnitude < cachedSqrMagnitude) {
        //            closestStepHit = _ignoredVictims;
        //            cachedSqrMagnitude = (closestStepHit.point - groundPosition).sqrMagnitude;
        //            continueTry = true;
        //        }
        //    }
        //    if (!continueTry) {
        //        return;
        //    }
        //    //Debug.DrawLine(closestStepHit.point, closestStepHit.point + closestStepHit.normal,Color.cyan);
        //    continueTry = false;
        //    var footHeightVector = Vector3.Project(groundPosition, _characterUp);
        //    origin = closestStepHit.point - 0.5f * _capsule.radius * direction;
        //    origin = Vector3.ProjectOnPlane(origin, _characterUp) + footHeightVector;
        //    origin += (0.5f * _capsule.height + maxStepHeight) * _characterUp;
        //    var stableStepHit = defaultAttacker(RaycastHit);
        //    hitAmount = CharacterCapsuleCastNonAlloc(origin,
        //                                             -_characterUp,
        //                                             out _hits,
        //                                             maxStepHeight);
        //    while (--hitAmount >= 0) {
        //        var _ignoredVictims = _hits[hitAmount];
        //        if (_ignoredVictims.distance == 0) {
        //            return;
        //        }
        //        if (_ignoredVictims.colliderInstanceID == closestStepHit.colliderInstanceID
        //            && IsStableOnNormal(_ignoredVictims.normal)) {
        //            stableStepHit = _ignoredVictims;
        //            continueTry = true;
        //            break;
        //        }
        //    }
        //    if (!continueTry) {
        //        return;
        //    }
        //    //var targetUpSpeed = (Vector3.Project(stableStepHit.point, _characterUp) - footHeightVector).magnitude * maxStepUpSpeed;
        //    //var currentUpSpeed = Vector3.Project(_rigidbody.velocity, _characterUp).magnitude;
        //    //if (currentUpSpeed < targetUpSpeed) {
        //    //    _rigidbody.AddForce((targetUpSpeed - currentUpSpeed) * _characterUp, ForceMode.VelocityChange);
        //    //}
        //    _rigidbody.MovePosition(_position += (stableStepHit.point.Project(_characterUp) - footHeightVector));
        //    //_rigidbody.AddForce(-currentVelocity.Project(_characterUp), ForceMode.VelocityChange);
        //    //var vc = (stableStepHit.point - groundPosition).Project(_characterUp) / Time.fixedDeltaTime;
        //    //_rigidbody.AddForce(vc, ForceMode.VelocityChange);
        //    //await UniTask.Yield(PlayerLoopTiming.FixedTick);
        //    //_rigidbody.AddForce(-vc, ForceMode.VelocityChange);
        //}

        /*
           if (groundStatus.IsStableOnGround) {
                    var originPoint = closestHit.point;
                    var planePoints = new List<Vector3>();
                    var radius = _capsule.radius;
                    var offset = 0.1f * radius * _characterForward + radius * _characterUp;
                    var quaternion = Quaternion.AngleAxis(72f, direction);
                    for (int i = 0; i < 5; ++i) {
                        if (Physics.Raycast(originPoint + offset, direction, out var hitInfo, 1.1f * radius, collisionMask, QueryTriggerInteraction.Ignore)) {
                            planePoints.Add(hitInfo.point);
                        }
                        offset = quaternion * offset;
                    }
                    if (planePoints.Count > 2) {
                        planePoints.Sort((a, b) => (Vector3.Distance(originPoint, a) > Vector3.Distance(originPoint, b)) ? 1 : -1);
                        var v1 = planePoints[1] - planePoints[0];
                        var v2 = planePoints[2] - planePoints[0];
                        var planeNormal = v1.Cross(v2).normalized;
                        if (planeNormal.Dot(_characterUp) < 0) {
                            planeNormal = -planeNormal;
                        }
                        groundStatus.GroundNormal = planeNormal;
                    } else if (planePoints.Count > 1) {
                        planePoints.Sort((a, b) => (Vector3.Distance(originPoint, a) > Vector3.Distance(originPoint, b)) ? 1 : -1);
                        var v1 = planePoints[0] - originPoint;
                        var v2 = planePoints[1] - originPoint;
                        var planeNormal = v1.Cross(v2).normalized;
                        if (planeNormal.Dot(_characterUp) < 0) {
                            planeNormal = -planeNormal;
                        }
                        groundStatus.GroundNormal = planeNormal;
                    }
                    Debug.DrawRay(groundStatus.GroundPoint, groundStatus.GroundNormal, Color.red);
                }
         */
    }
}