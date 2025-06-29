using EditorAttributes;
using GuestUnion;
using JoG.Character.InputBanks;
using JoG.Messages;
using MessagePipe;
using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

namespace JoG.CameraSystem {

    public class FreeCameraController : MonoBehaviour, IMessageHandler<CharacterBodyChangedMessage>, IEye {
        private float cameraEulerX, cameraEulerY, cameraEulerZ;
        private Transform _cachedTrackingTarget;
        private Transform _positionFollow;
        private IDisposable _disposable;
        private InputAction _lookInput;
        private Vector3InputBank _aimInputBank;
        [field: SerializeField, Required] public CinemachineCamera ThirdPersonCamera { get; private set; }
        [field: SerializeField, Required] public CinemachineThirdPersonAim ThirdPersonAim { get; private set; }

        public Transform Follow { get => _positionFollow; set => _positionFollow = value; }

        public Vector3 AimTarget => ThirdPersonAim.AimTarget;

        public Vector3 AimPosition { get; private set; }

        public Vector3 AimOrigin { get; private set; }

        public Vector3 AimVector3 { get; private set; }

        public GameObject AimObject { get; private set; }

        void IMessageHandler<CharacterBodyChangedMessage>.Handle(CharacterBodyChangedMessage message) {
            if (message.next != null) {
                Follow = message.next.AimOriginTransform;
                _aimInputBank = message.next.GetInputBank<Vector3InputBank>("Aim");
            } else {
                Follow = null;
                _aimInputBank = null;
            }
        }

        [Inject]
        private void Construct(IBufferedSubscriber<CharacterBodyChangedMessage> subscriber) {
            _disposable = subscriber.Subscribe(this);
        }

        private void Awake() {
            _cachedTrackingTarget = ThirdPersonCamera.Follow;
            _lookInput = InputSystem.actions.FindAction("Look", true);
        }

        private void OnEnable() {
            _lookInput.performed += OnLook;
        }

        private void Update() {

            AimPosition = ThirdPersonAim.AimTarget;
            AimOrigin = ThirdPersonCamera.State.GetFinalPosition();
            AimVector3 = AimPosition - AimOrigin;
            Physics.Raycast(AimOrigin, AimVector3, out var hit, 1000, ThirdPersonAim.AimCollisionFilter, QueryTriggerInteraction.Ignore);
            AimObject = hit.collider?.gameObject;
            if (_positionFollow == null) return;
            _cachedTrackingTarget.position = _positionFollow.position;
            _aimInputBank.vector3 = ThirdPersonAim.AimTarget;
        }

        private void OnDisable() {
            _lookInput.performed -= OnLook;
        }

        private void OnDestroy() {
            _disposable?.Dispose();
        }

        private void OnLook(InputAction.CallbackContext context) {
            var rotateVector2 = context.ReadValue<Vector2>();
            cameraEulerX = (cameraEulerX + rotateVector2.y).Clamp(-89, 89);
            cameraEulerY = (cameraEulerY + rotateVector2.x).NormalizeAngleOne360();
            _cachedTrackingTarget.rotation = Quaternion.Euler(cameraEulerX, cameraEulerY, cameraEulerZ);
        }
    }
}