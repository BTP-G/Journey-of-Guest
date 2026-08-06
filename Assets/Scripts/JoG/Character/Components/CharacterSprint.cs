using JoG.Character.InputBanks;
using Xoderony.Numerics;
using UnityEngine;
using VContainer;

namespace JoG.Character.Components {

    public class CharacterSprint : MonoBehaviour, IComponent {
        public float sprintSpeedMultiplier = 1.3f;
        [Inject] internal Animator _animator;
        [Inject, Key(Constants.Stats.MaxMoveSpeed)] internal Stat _maxMoveSpeed;
        [Inject, Key(Constants.Stats.MoveAcceleration)] internal Stat _acceleration;
        private BooleanInputBank sprintInput;

        private bool _isSprinting;
        private int _accelerationMultiplierSlotIndex;
        private int _maxMoveSpeedMultiplierSlotIndex;

        public bool IsSprinting => _isSprinting;

        public void StartSprinting() {
            if (_isSprinting) {
                return;
            }

            var multiplier = new Q16(sprintSpeedMultiplier);
            _accelerationMultiplierSlotIndex = _acceleration.AcquireMultiplierSlot(multiplier);
            _maxMoveSpeedMultiplierSlotIndex = _maxMoveSpeed.AcquireMultiplierSlot(multiplier);
            _isSprinting = true;
        }

        public void StopSprinting() {
            if (!_isSprinting) {
                return;
            }

            _acceleration.ReleaseMultiplierSlot(_accelerationMultiplierSlotIndex);
            _maxMoveSpeed.ReleaseMultiplierSlot(_maxMoveSpeedMultiplierSlotIndex);
            _isSprinting = false;
        }

        [Inject]
        internal void Inject(InputBankHub inputBankHub) {
            sprintInput = inputBankHub.GetInputBank<SprintInputBank>();
        }

        protected void Update() {
            if (sprintInput.Value) {
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
