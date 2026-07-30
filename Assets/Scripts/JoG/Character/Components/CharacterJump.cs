using EditorAttributes;
using Xoderony.Movement;
using JoG.Character;
using JoG.Character.InputBanks;
using System.Runtime.CompilerServices;
using UnityEngine;
using VContainer;

namespace JoG.Character.Components {

    [DisallowMultipleComponent, DefaultExecutionOrder(-1)]
    public class CharacterJump : MonoBehaviour,IComponent {

        [Tooltip("起跳速度")]
        public float jumpSpeed = 5f;

        public int forceNotGroundedFrames = 3;
        [Inject] internal CharacterMotor motor;
        [Inject] internal InputBankHub _inputBankHub;

        private JumpInputBank jumpInput;

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
        internal void Inject(InputBankHub inputBankHub) {
            jumpInput = _inputBankHub.GetInputBank<JumpInputBank>();
        }

        private void Update() {
            if (jumpInput.Value && motor.IsStable) {
                Jump();
                jumpInput.UpdateState(false);
            }
        }
    }
}
