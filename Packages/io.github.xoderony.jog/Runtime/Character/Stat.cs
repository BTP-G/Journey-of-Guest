using Xoderony.Numerics;
using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace JoG.Character {

    [Serializable]
    public class Stat : IComponent, ISerializationCallbackReceiver {

        private const int InitialMultiplierSlotCount = 2;

        private const int MaxMultiplierSlotCount = 128;

        [SerializeField]
        private string _name;

        [SerializeField]
        private int _baseValue;

        [SerializeField]
        private int _minValue = int.MinValue;

        [SerializeField]
        private int _maxValue = int.MaxValue;

        [NonSerialized]
        private int _value;

        [NonSerialized]
        private Q16[] _multiplierSlots;

        [NonSerialized]
        private int[] _freeSlotIndices;

        [NonSerialized]
        private int _freeSlotCount;

        object IComponent.Key => _name;

        public string Name => _name;

        public int Value => _value;

        [field: NonSerialized]
        public event Action ValueChanged;

        public Stat() {
            ResetMultiplierSlots();
        }

        public Stat(string name, int baseValue, int minValue, int maxValue) {
            _name = name;
            _baseValue = baseValue;
            _minValue = minValue;
            _maxValue = maxValue;
            ResetMultiplierSlots();
            NormalizeSerializedValues();
            Recalculate();
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize() {
            NormalizeSerializedValues();
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize() {
            ResetMultiplierSlots();
            NormalizeSerializedValues();
            Recalculate();
        }

        public int AcquireMultiplierSlot(Q16 multiplier) {
            Assert.IsTrue(multiplier > Q16.Zero);
            if (_freeSlotCount == 0) {
                GrowByOne();
            }

            _freeSlotCount--;
            var slotIndex = _freeSlotIndices[_freeSlotCount];
            _multiplierSlots[slotIndex] = multiplier;
            Recalculate();
            return slotIndex;
        }

        public void ReleaseMultiplierSlot(int slotIndex) {
            ValidateSlotIndex(slotIndex);
            _multiplierSlots[slotIndex] = Q16.One;
            _freeSlotIndices[_freeSlotCount] = slotIndex;
            _freeSlotCount++;
            Recalculate();
        }

        public void SetMultiplier(int slotIndex, Q16 multiplier) {
            Assert.IsTrue(multiplier > Q16.Zero);
            ValidateSlotIndex(slotIndex);
            _multiplierSlots[slotIndex] = multiplier;
            Recalculate();
        }

        private void GrowByOne() {
            var oldLength = _multiplierSlots.Length;
            var newLength = oldLength + 1;
            if (newLength > MaxMultiplierSlotCount) {
                throw new InvalidOperationException($"Exceeded max multiplier slot capacity ({newLength}).");
            }

            var newMultiplierSlots = new Q16[newLength];
            Array.Copy(_multiplierSlots, newMultiplierSlots, oldLength);
            newMultiplierSlots[oldLength] = Q16.One;
            _multiplierSlots = newMultiplierSlots;

            var newFreeSlotIndices = new int[newLength];
            Array.Copy(_freeSlotIndices, newFreeSlotIndices, _freeSlotCount);
            newFreeSlotIndices[_freeSlotCount] = oldLength;
            _freeSlotIndices = newFreeSlotIndices;
            _freeSlotCount++;
        }

        private void NormalizeSerializedValues() {
            if (_maxValue < _minValue) {
                _maxValue = _minValue;
            }

            _baseValue = Math.Clamp(_baseValue, _minValue, _maxValue);
        }

        private void Recalculate() {
            var value = (long)_baseValue;
            foreach (var multiplier in _multiplierSlots) {
                value = multiplier.Multiply(value);
            }

            value = Math.Clamp(value, _minValue, _maxValue);
            var nextValue = (int)value;
            if (_value == nextValue) {
                return;
            }

            _value = nextValue;
            ValueChanged?.Invoke();
        }

        private void ResetMultiplierSlots() {
            _multiplierSlots = new Q16[InitialMultiplierSlotCount];
            _freeSlotIndices = new int[InitialMultiplierSlotCount];
            for (var i = 0; i < InitialMultiplierSlotCount; i++) {
                _multiplierSlots[i] = Q16.One;
                _freeSlotIndices[i] = i;
            }

            _freeSlotCount = InitialMultiplierSlotCount;
        }

        private void ValidateSlotIndex(int slotIndex) {
            if ((uint)slotIndex >= (uint)_multiplierSlots.Length) {
                throw new ArgumentOutOfRangeException(nameof(slotIndex));
            }
        }

    }

}
