using EditorAttributes;
using System.Runtime.CompilerServices;
using UnityEngine;
using VContainer;
using Xoderony.Movement;
using Xoderony.InputChannels;

namespace JoG.Character.Components {

    [DisallowMultipleComponent, DefaultExecutionOrder(-1)]
    public class CharacterJump : MonoBehaviour, IComponent {

        [Tooltip("起跳速度")]
        public float jumpSpeed = 5f;

        public int forceNotGroundedFrames = 3;
        [Inject] internal CharacterMotor motor;
        [Inject] internal InputChannelHub _inputChannelHub;

        private InputChannel<bool> jumpInput;

        public float JumpHeight {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => jumpSpeed * jumpSpeed / (2 * Physics.gravity.magnitude);
            [Button]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => jumpSpeed = Mathf.Sqrt(2 * Physics.gravity.magnitude * value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Jump() {
            motor.ExternalVelocity += jumpSpeed * motor.Up;
            motor.ForceUngrounded(5);
        }

        [Inject]
        internal void Inject(InputChannelHub inputChannelHub) {
            jumpInput = _inputChannelHub.GetInputChannel<bool>(InputKeys.Jump);
        }

        private void Update() {
            if (jumpInput.value && motor.IsStable) {
                Jump();
                jumpInput.value = false;
            }
        }
    }
}
