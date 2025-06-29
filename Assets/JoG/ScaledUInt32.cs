using System;
using UnityEngine;

namespace JoG {

    [Serializable]
    public struct ScaledUInt32 : IComparable<ScaledUInt32>, IEquatable<ScaledUInt32>, ISerializationCallbackReceiver {
        [SerializeField] private uint _baseValue;

        [SerializeField] private float _multiplier;

        private uint _value;

        public uint BaseValue {
            readonly get => _baseValue;
            set {
                _baseValue = value;
                _value = (uint)(_baseValue * _multiplier);
            }
        }

        public float Multiplier {
            readonly get => _multiplier;
            set {
                _multiplier = value;
                _value = (uint)(_baseValue * _multiplier);
            }
        }

        public readonly uint Value => _value;

        public ScaledUInt32(uint baseValue, float multiplier = 1f) {
            _baseValue = baseValue;
            _multiplier = multiplier;
            _value = (uint)(baseValue * multiplier);
        }

        public static implicit operator uint(in ScaledUInt32 single) => single._value;

        public static implicit operator ScaledUInt32(uint value) => new(value);

        void ISerializationCallbackReceiver.OnAfterDeserialize() => _value = (uint)(_baseValue * _multiplier);

        readonly void ISerializationCallbackReceiver.OnBeforeSerialize() {
        }

        public readonly int CompareTo(ScaledUInt32 other) => _value.CompareTo(other._value);

        public readonly bool Equals(ScaledUInt32 other) => _baseValue == other._baseValue && _multiplier == other._multiplier;
    }
}