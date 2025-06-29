using UnityEngine;

namespace JoG.Character.Move {

    [RequireComponent(typeof(RigidbodyCharacterController))]
    public class RigidbodyCrouch : MonoBehaviour, ICrouch {
        public float crouchSpeed;
        [SerializeField] private float _crouchHeightMultiplier = 0.65f;
        [SerializeField] private float _crouchSpeedMultiplierAddend = -0.5f;
        private RigidbodyCharacterController _characterController;
        private bool _isCrouching;
        private float _targetHeight;
        //private CancellationTokenSource _tokenSource;

        public RigidbodyCharacterController CharacterController => _characterController;
        public float TargetHeight => _targetHeight;

        public float CrouchHeightMultiplier {
            get => _crouchHeightMultiplier;
            set {
                IsCrouching = false;
                _crouchHeightMultiplier = value;
            }
        }

        public bool IsCrouching {
            get => _isCrouching;
            set {
                if (_isCrouching ^ value) {
                    enabled = true;
                    if (_isCrouching = value) {
                        _characterController.maxStableMoveSpeed.Multiplier += _crouchSpeedMultiplierAddend;
                        //SmoothMoveCapsuleHeight(_capsuleHeight, _capsuleHeight *= _crouchHeightMultiplier, crouchTime);
                        _targetHeight *= _crouchHeightMultiplier;
                    } else {
                        _characterController.maxStableMoveSpeed.Multiplier -= _crouchSpeedMultiplierAddend;
                        //SmoothMoveCapsuleHeight(_capsuleHeight, _capsuleHeight /= _crouchHeightMultiplier, crouchTime);
                        _targetHeight /= _crouchHeightMultiplier;
                    }
                }
            }
        }

        public float SprintSpeedMultiplierAddend {
            get => _crouchSpeedMultiplierAddend;
            set {
                if (_isCrouching) {
                    _characterController.maxStableMoveSpeed.Multiplier -= _crouchSpeedMultiplierAddend - value;
                }
                _crouchSpeedMultiplierAddend = value;
            }
        }

        //public async void SmoothMoveCapsuleHeight(float current, float defaultFollow, float time) {
        //    _tokenSource?.CancelButton();
        //    var tokenSource = _tokenSource = new CancellationTokenSource();
        //    var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_tokenSource.Token, destroyCancellationToken);
        //    var maxDelta = Math.Abs(defaultFollow - current) / time;
        //    do {
        //        current = Mathf.MoveTowards(current, defaultFollow, maxDelta * Time.deltaTime);
        //        var newCenter = _characterController.CapsuleCenter;
        //        newCenter.y += 0.5f * (current - _characterController.CapsuleHeight);
        //        _characterController.UpdateCapsule(newCenter, current);
        //    } while (current != defaultFollow
        //    && !await UniTask.Yield(linkedTokenSource.Token).SuppressCancellationThrow());
        //    tokenSource.Dispose();
        //    if (_tokenSource == tokenSource) {
        //        _tokenSource = null;
        //    }
        //    linkedTokenSource.Dispose();
        //}

        protected void Awake() {
            _characterController = GetComponent<RigidbodyCharacterController>();
        }

        protected void Start() {
            _targetHeight = _characterController.CapsuleHeight;
        }

        protected void Update() {
            var current = _characterController.CapsuleHeight;
            if (current != _targetHeight) {
                var newHeight = Mathf.MoveTowards(current, _targetHeight, crouchSpeed * Time.deltaTime);
                _characterController.CapsuleHeight = newHeight;
            } else {
                enabled = false;
            }
        }
    }
}