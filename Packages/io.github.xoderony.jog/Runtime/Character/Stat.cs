using Xoderony.Numerics;
using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace JoG.Character {

    [Serializable]
    public class Stat : StatBase<Q16>, IStat {

        [SerializeField]
        private int _baseValue;

        [SerializeField]
        private int _minValue = int.MinValue;

        [SerializeField]
        private int _maxValue = int.MaxValue;

        [NonSerialized]
        private int _value;

        public int Value => _value;

        public Stat() : base(Q16.One) {
        }

        public Stat(string name, int baseValue, int minValue, int maxValue) : base(name, Q16.One) {
            _baseValue = baseValue;
            _minValue = minValue;
            _maxValue = maxValue;
            NormalizeSerializedValues();
            Recalculate();
        }

        public int AcquireMultiplierSlot(float multiplier) {
            var q16Multiplier = new Q16(multiplier);
            Assert.IsTrue(q16Multiplier > Q16.Zero);
            var slotIndex = AcquireMultiplierSlotCore(q16Multiplier);
            Recalculate();
            return slotIndex;
        }

        public int AcquireMultiplierSlot(Q16 multiplier) {
            Assert.IsTrue(multiplier > Q16.Zero);
            var slotIndex = AcquireMultiplierSlotCore(multiplier);
            Recalculate();
            return slotIndex;
        }

        public void ReleaseMultiplierSlot(int slotIndex) {
            ReleaseMultiplierSlotCore(slotIndex);
            Recalculate();
        }

        public void SetMultiplier(int slotIndex, float multiplier) {
            var q16Multiplier = new Q16(multiplier);
            Assert.IsTrue(q16Multiplier > Q16.Zero);
            SetMultiplierCore(slotIndex, q16Multiplier);
            Recalculate();
        }

        public void SetMultiplier(int slotIndex, Q16 multiplier) {
            Assert.IsTrue(multiplier > Q16.Zero);
            SetMultiplierCore(slotIndex, multiplier);
            Recalculate();
        }

        protected override void Recalculate() {
            var value = (long)_baseValue;
            foreach (var multiplier in MultiplierSlots) {
                value = multiplier.Multiply(value);
            }

            value = Math.Clamp(value, _minValue, _maxValue);
            var nextValue = (int)value;
            if (_value == nextValue) {
                return;
            }

            _value = nextValue;
            RaiseValueChanged();
        }

        protected override void NormalizeSerializedValues() {
            if (_maxValue < _minValue) {
                _maxValue = _minValue;
            }

            _baseValue = Math.Clamp(_baseValue, _minValue, _maxValue);
        }

    }

}
