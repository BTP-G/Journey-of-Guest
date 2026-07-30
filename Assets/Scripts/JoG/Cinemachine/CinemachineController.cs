using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

namespace JoG.Cinemachine {

    public class CinemachineController : InputAxisControllerBase<CinemachineController.Reader>, IComponent, INetworkOwnershipChangeHandler {
        [Inject, Key(Constants.InputAction.Look)] internal InputAction lookInput;
        [Inject, Key(Constants.InputAction.Scroll)] internal InputAction scrollInput;

        public CinemachineCamera CinemachineCamera { get; private set; }

        void INetworkOwnershipChangeHandler.OnGainedOwnership(bool isNewOwner) {
            CinemachineCamera.Priority.Value = Convert.ToInt32(isNewOwner);
        }

        void INetworkOwnershipChangeHandler.OnLostOwnership(bool isPreviousOwner) {
        }

        private void Awake() {
            CinemachineCamera = GetComponent<CinemachineCamera>();
        }

        private void Update() {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif
            UpdateControllers();
        }

        [Serializable]
        public sealed class Reader : IInputAxisReader {

            float IInputAxisReader.GetValue(UnityEngine.Object context, IInputAxisOwner.AxisDescriptor.Hints hint) {
                var inputer = context as CinemachineController;
                return hint switch {
                    IInputAxisOwner.AxisDescriptor.Hints.X => inputer.lookInput.ReadValue<Vector2>().x / Time.deltaTime,
                    IInputAxisOwner.AxisDescriptor.Hints.Y => inputer.lookInput.ReadValue<Vector2>().y / Time.deltaTime,
                    _ => inputer.scrollInput.ReadValue<float>(),
                };
            }
        }
    }
}
