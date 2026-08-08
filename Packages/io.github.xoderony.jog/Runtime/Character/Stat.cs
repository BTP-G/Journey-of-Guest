using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Assertions;
using Xoderony.Numerics;

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

        [NonSerialized]
        private bool _dirty;

        object IComponent.Key => _name;

        public string Name => _name;

        public int Value {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get {
                if (_dirty) {
                    Recalculate();
                }
                return _value;
            }
        }

        // 写路径（Add/Release/Set）标脏后立即触发：值可能已变化，订阅者在回调中读取 Value 获取最新值。
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StatModifier AddModifier(Q16 multiplier) {
            var slotIndex = AcquireMultiplierSlot(multiplier);
            return new StatModifier(this, slotIndex);
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize() {
            NormalizeSerializedValues();
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize() {
            ResetMultiplierSlots();
            NormalizeSerializedValues();
            Recalculate();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal int AcquireMultiplierSlot(Q16 multiplier) {
            Assert.IsTrue(multiplier > Q16.Zero);
            if (_freeSlotCount == 0) {
                GrowByOne();
            }

            _freeSlotCount--;
            var slotIndex = _freeSlotIndices[_freeSlotCount];
            _multiplierSlots[slotIndex] = multiplier;
            _dirty = true;
            ValueChanged?.Invoke();
            return slotIndex;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void ReleaseMultiplierSlot(int slotIndex) {
            ValidateSlotIndex(slotIndex);
            _multiplierSlots[slotIndex] = Q16.One;
            _freeSlotIndices[_freeSlotCount] = slotIndex;
            _freeSlotCount++;
            _dirty = true;
            ValueChanged?.Invoke();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void SetMultiplier(int slotIndex, Q16 multiplier) {
            Assert.IsTrue(multiplier > Q16.Zero);
            ValidateSlotIndex(slotIndex);
            _multiplierSlots[slotIndex] = multiplier;
            _dirty = true;
            ValueChanged?.Invoke();
        }

        private void GrowByOne() {
            var oldLength = _multiplierSlots.Length;
            var newLength = oldLength + 1;
            Assert.IsTrue(newLength <= MaxMultiplierSlotCount, $"Exceeded max multiplier slot capacity ({newLength}).");
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void NormalizeSerializedValues() {
            if (_maxValue < _minValue) {
                _maxValue = _minValue;
            }

            _baseValue = Math.Clamp(_baseValue, _minValue, _maxValue);
        }

        private void Recalculate() {
            _dirty = false;
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
        }

        private void ResetMultiplierSlots() {
            _multiplierSlots = new Q16[InitialMultiplierSlotCount];
            _freeSlotIndices = new int[InitialMultiplierSlotCount];
            for (var i = 0; i < InitialMultiplierSlotCount; i++) {
                _multiplierSlots[i] = Q16.One;
                _freeSlotIndices[i] = i;
            }

            _freeSlotCount = InitialMultiplierSlotCount;
            _dirty = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ValidateSlotIndex(int slotIndex) {
            Assert.IsTrue((uint)slotIndex < (uint)_multiplierSlots.Length, $"Invalid multiplier slot index: {slotIndex}.");
        }
    }
}
