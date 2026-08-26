using UnityEngine;
using VContainer;
using Xoderony.Numerics;
using Xoderony.InputChannels;

namespace Expriverse.Character.Components {

    public class CharacterSprint : MonoBehaviour, IComponent {
        public float sprintSpeedMultiplier = 1.3f;
        [Inject] internal Animator _animator;
        [Inject, Key(Constants.Stats.MaxMoveSpeed)] internal Stat _maxMoveSpeed;
        [Inject, Key(Constants.Stats.MoveAcceleration)] internal Stat _acceleration;
        private InputChannel<bool> sprintInput;

        private bool _isSprinting;

        private StatModifier _accelerationModifier;

        private StatModifier _maxMoveSpeedModifier;

        public bool IsSprinting => _isSprinting;

        public void StartSprinting() {
            if (_isSprinting) {
                return;
            }

            var multiplier = new Q16(sprintSpeedMultiplier);
            _accelerationModifier = _acceleration.AddModifier(multiplier);
            _maxMoveSpeedModifier = _maxMoveSpeed.AddModifier(multiplier);
            _isSprinting = true;
        }

        public void StopSprinting() {
            if (!_isSprinting) {
                return;
            }

            _accelerationModifier.Remove();
            _maxMoveSpeedModifier.Remove();
            _isSprinting = false;
        }

        [Inject]
        internal void Inject(InputChannelHub inputChannelHub) {
            sprintInput = inputChannelHub.GetInputChannel<bool>(InputKeys.Sprint);
        }

        protected void Update() {
            if (sprintInput.value) {
                if (!_isSprinting) {
                    StartSprinting();
                }
            } else {
                if (_isSprinting) {
                    StopSprinting();
                }
            }
        }

        protected void OnDisable() {
            StopSprinting();
        }
    }
}
