using System;
using UnityEngine;

namespace JoG {

    [Serializable]
    public struct ScaledSingle : IComparable<ScaledSingle>, IEquatable<ScaledSingle>, ISerializationCallbackReceiver {
        [SerializeField] private float _baseValue;

        [SerializeField] private float _multiplier;

        private float _value;

        public float BaseValue {
            readonly get => _baseValue;
            set {
                _baseValue = value;
                _value = _baseValue * _multiplier;
            }
        }

        public float Multiplier {
            readonly get => _multiplier;
            set {
                _multiplier = value;
                _value = _baseValue * _multiplier;
            }
        }

        public readonly float Value => _value;

        public ScaledSingle(float baseValue, float multiplier = 1f) {
            _baseValue = baseValue;
            _multiplier = multiplier;
            _value = baseValue * multiplier;
        }

        public static implicit operator float(in ScaledSingle single) => single._value;

        public static implicit operator ScaledSingle(float value) => new(value);

        void ISerializationCallbackReceiver.OnAfterDeserialize() => _value = _baseValue * _multiplier;

        readonly void ISerializationCallbackReceiver.OnBeforeSerialize() {
        }

        public readonly int CompareTo(ScaledSingle other) => _value.CompareTo(other._value);

        public readonly bool Equals(ScaledSingle other) => _baseValue == other._baseValue && _multiplier == other._multiplier;
    }
}