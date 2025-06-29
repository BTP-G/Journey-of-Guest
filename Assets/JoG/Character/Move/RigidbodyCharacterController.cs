using GuestUnion;
using UnityEngine;

namespace JoG.Character.Move {

    public delegate void MoveEventHandler(RigidbodyCharacterController sender, float deltaTime);

    public partial class RigidbodyCharacterController : MonoBehaviour, ICharacterController {

        public void UpdateMoveDirection(in Vector3 moveDirection) {
            this.moveDirection = moveDirection;
        }

        public void UpdateAimDirection(in Vector3 aimDirection) {
            this.aimDirection = aimDirection;
        }

        void ICharacterController.Enable() {
            enabled = true;
        }

        void ICharacterController.Disable() {
            enabled = false;
        }

        private void UpdateRotation(float deltaTime) {
            _rotation = _rigidbody.rotation;
            _characterUp = _rotation * Vector3.up;
            _characterForward = _rotation * Vector3.forward;
            _characterRight = _rotation * Vector3.right;
            if (zTowardsAimDirection) {
                var toDirection = Vector3.ProjectOnPlane(aimDirection, _characterUp);
                toDirection = Vector3.Slerp(_characterForward, toDirection, 1 - Mathf.Exp(-forwardOrientationSharpness * deltaTime));
                _rotation = Quaternion.LookRotation(toDirection, _characterUp);
            } else {
                if (moveDirection.sqrMagnitude > Vector3.kEpsilonNormalSqrt) {
                    var toDirection = Vector3.Slerp(_characterForward, moveDirection, 1 - Mathf.Exp(-forwardOrientationSharpness * deltaTime));
                    _rotation = Quaternion.LookRotation(toDirection, _characterUp);
                    //_rotation = Quaternion.FromToRotation(_characterForward, toDirection) * _rotation;
                }
            }
            if (_angularVelocityFromGround.sqrMagnitude > Vector3.kEpsilonNormalSqrt) {
                _rotation = Quaternion.Euler(Mathf.Rad2Deg * deltaTime * _angularVelocityFromGround) * _rotation;
            }
            if (yTowardsGravity) {
                var toDirection = Vector3.Slerp(_characterUp, -gravity.normalized, 1 - Mathf.Exp(-downOrientationSharpness * deltaTime));
                _rotation = Quaternion.FromToRotation(_rotation * Vector3.up, toDirection) * _rotation;
            } else {
                var toDirection = Vector3.Slerp(_characterUp, groundStatus.IsStableOnGround ? groundStatus.GroundNormal : -gravity.normalized, 1 - Mathf.Exp(-downOrientationSharpness * deltaTime));
                _rotation = Quaternion.FromToRotation(_rotation * Vector3.up, toDirection) * _rotation;
            }
            _rigidbody.MoveRotation(_rotation);
            _characterUp = _rotation * Vector3.up;
            _characterForward = _rotation * Vector3.forward;
            _characterRight = _rotation * Vector3.right;
        }

        private void UpdateVelocity(float deltaTime) {
            if (groundStatus.IsStableOnGround) {
                var groundNormal = groundStatus.GroundNormal;
                var moveDirection = GetDirectionTangentToSurface(this.moveDirection, groundNormal);
                var targetVelocity = (maxStableMoveSpeed * moveDirection.ProjectOnPlane(_characterUp)) + _velocityFromGround;
                var velocityFromGroundOnUp = _velocityFromGround.Project(_characterUp);
                if (velocityFromGroundOnUp.sqrMagnitude > 0f) {
                    var v = velocityFromGroundOnUp - currentVelocity.Project(_characterUp);
                    _rigidbody.AddForce(v, ForceMode.VelocityChange);
                    currentVelocity += v;
                }
                if (groundStatus.DistanceToGround != maxStepHeight) {
                    var floatDistance = groundStatus.DistanceToGround - maxStepHeight;
                    var maxFloatSpeed = (floatDistance > 0f) ? floatDistance / deltaTime : -floatDistance / deltaTime;
                    var reachGroundVelocity = Mathf.Clamp(floatDistance / maxStepHeight * maxStableMoveSpeed, -maxFloatSpeed, maxFloatSpeed) * -_characterUp;
                    targetVelocity += reachGroundVelocity;
                    var v = reachGroundVelocity + velocityFromGroundOnUp - currentVelocity.Project(_characterUp);
                    _rigidbody.AddForce(v, ForceMode.VelocityChange);
                    currentVelocity += v;
                }
                if (targetVelocity != currentVelocity) {
                    var v = Vector3.ClampMagnitude(targetVelocity - currentVelocity, acceleration * deltaTime);
                    _rigidbody.AddForce(v, ForceMode.VelocityChange);
                    currentVelocity += v;
                }
            } else {
                var f = Mathf.Clamp01(1f - (currentVelocity.magnitude / maxStableMoveSpeed));
                _rigidbody.AddForce((f * acceleration * airControl * moveDirection) + gravity, ForceMode.Acceleration);
            }
        }

        private void Awake() {
            _hits = arrayPool.Rent(MaxHitsBudget);
        }

        private void Start() {
            UpdateCapsuleTopAndBottomOffset();
        }

        private void FixedUpdate() {
            _position = _rigidbody.position;
            lastVelocity = currentVelocity;
            currentVelocity = _rigidbody.linearVelocity;
            var fixedDeltaTime = Time.fixedDeltaTime;
            UpdateGroundStatus();
            if (_groundRigidbody.TryGet(out var rigidbody)) {
                GetPointVelocityFromRigidbody(rigidbody, groundStatus.GroundPoint, out _velocityFromGround, out _angularVelocityFromGround);
            } else {
                _velocityFromGround = default;
                _angularVelocityFromGround = default;
            }
            UpdateVelocity(fixedDeltaTime);
        }

        private void Update() {
            UpdateRotation(Time.deltaTime);
        }

        private void OnDestroy() {
            arrayPool.Return(_hits);
        }
    }
}