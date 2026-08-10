using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Assertions;
using Xoderony.Collections;
using Xoderony.Numerics;

namespace JoG.Character {

    [Serializable]
    public class Stat : IComponent, ISerializationCallbackReceiver {

        private const int InitialModifierSlotCount = 2;

        private const int MaxModifierSlotCount = 128;

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

        // 密集槽位：只有前 Count 个元素有效；_multiplierSlots 与之一一对应并保持同步。
        [NonSerialized]
        private ArrayList<StatModifier> _modifierSlots;

        [NonSerialized]
        private Q16[] _multiplierSlots;

        object IComponent.Key => _name;

        public string Name => _name;

        public int Value {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _value;
        }

        // 写路径（Add/Release/Set）立即重算并触发：订阅者在回调中读取 Value 即为最新值。
        [field: NonSerialized]
        public event Action ValueChanged;

        public Stat() : this(null, 0, int.MinValue, int.MaxValue) {
        }

        public Stat(string name, int baseValue, int minValue, int maxValue) {
            _name = name;
            _baseValue = baseValue;
            _minValue = minValue;
            _maxValue = maxValue;
            ResetRuntimeState();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StatModifier AddModifier(Q16 multiplier) {
            Assert.IsTrue(multiplier > Q16.Zero);
            var slotIndex = _modifierSlots.Count;
            EnsureMultiplierCapacity(slotIndex + 1);
            var modifier = new StatModifier(this, slotIndex);
            _modifierSlots.Add(modifier);
            _multiplierSlots[slotIndex] = multiplier;
            Recalculate();
            ValueChanged?.Invoke();
            return modifier;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void SetMultiplier(int slotIndex, Q16 multiplier) {
            Assert.IsTrue(multiplier > Q16.Zero);
            Assert.IsTrue((uint)slotIndex < (uint)_modifierSlots.Count, $"Invalid modifier slot index: {slotIndex}.");
            _multiplierSlots[slotIndex] = multiplier;
            Recalculate();
            ValueChanged?.Invoke();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void RemoveMultiplierSlot(int slotIndex) {
            Assert.IsTrue((uint)slotIndex < (uint)_modifierSlots.Count, $"Invalid modifier slot index: {slotIndex}.");
            var lastIndex = _modifierSlots.Count - 1;
            // 移除最后一个槽位时三条赋值均为自赋值，可无分支统一处理。
            _modifierSlots[slotIndex] = _modifierSlots[lastIndex];
            _modifierSlots[slotIndex].SlotIndex = slotIndex;
            _multiplierSlots[slotIndex] = _multiplierSlots[lastIndex];
            _modifierSlots.RemoveAt(lastIndex);
            Recalculate();
            ValueChanged?.Invoke();
        }

        // 序列化前统一归一化，避免把非法范围写入数据。
        void ISerializationCallbackReceiver.OnBeforeSerialize() {
            NormalizeSerializedValues();
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize() {
            ResetRuntimeState();
        }

        private void EnsureMultiplierCapacity(int capacity) {
            Assert.IsTrue(capacity <= MaxModifierSlotCount, $"Exceeded max modifier slot capacity ({capacity}).");
            if (capacity <= _multiplierSlots.Length) {
                return;
            }
            var newCapacity = Math.Max(_multiplierSlots.Length * 2, capacity);
            Array.Resize(ref _multiplierSlots, newCapacity);
        }

        // 序列化字段的归一化只在构造/反序列化与序列化前执行；运行时字段保持不变。
        private void NormalizeSerializedValues() {
            if (_maxValue < _minValue) {
                _maxValue = _minValue;
            }
            _baseValue = Math.Clamp(_baseValue, _minValue, _maxValue);
        }

        // 纯重算：序列化字段已归一化且运行时不变，直接按槽位连乘并夹取。
        private void Recalculate() {
            var value = (long)_baseValue;
            // 只有前 Count 个槽位有效，尾部残留值不参与计算。
            var count = _modifierSlots.Count;
            for (var i = 0; i < count; i++) {
                value = _multiplierSlots[i].Multiply(value);
            }
            _value = (int)Math.Clamp(value, _minValue, _maxValue);
        }

        // 构造与反序列化后统一重建运行时槽位，并归一化序列化字段后重算。
        private void ResetRuntimeState() {
            _modifierSlots = new ArrayList<StatModifier>(InitialModifierSlotCount);
            _multiplierSlots = new Q16[InitialModifierSlotCount];
            NormalizeSerializedValues();
            Recalculate();
        }
    }
}
