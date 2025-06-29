using GuestUnion;
using System;
using UnityEngine;

namespace JoG {

    [Serializable]
    public struct RangeScaledSingle : IComparable<RangeScaledSingle>, IEquatable<RangeScaledSingle>, ISerializationCallbackReceiver {
        [SerializeField] private float _baseValue;
        [SerializeField] private float _multiplier;
        [SerializeField] private float _min;
        [SerializeField] private float _max;
        private float _value;

        public float BaseValue {
            readonly get => _baseValue;
            set {
                _baseValue = value;
                _value = (value * _multiplier).Clamp(_min, _max);
            }
        }

        public float Multiplier {
            readonly get => _multiplier;
            set {
                _multiplier = value;
                _value = (_baseValue * value).Clamp(_min, _max);
            }
        }

        public float Min {
            readonly get => _min;
            set {
                _min = value;
                _value = _value.Clamp(value, _max);
            }
        }

        public float Max {
            readonly get => _max;
            set {
                _max = value;
                _value = _value.Clamp(_min, value);
            }
        }

        public readonly float Value => _value;

        public RangeScaledSingle(float baseValue, float multiplier = 1, float min = float.MinValue, float max = float.MaxValue) : this() {
            _baseValue = baseValue;
            _multiplier = multiplier;
            _min = min;
            _max = max;
            _value = (baseValue * multiplier).Clamp(_min, _max);
        }

        public static implicit operator float(in RangeScaledSingle single) => single._value;

        public static implicit operator RangeScaledSingle(float value) => new(value);

        public readonly int CompareTo(RangeScaledSingle other) => _value.CompareTo(other._value);

        public readonly bool Equals(RangeScaledSingle other) =>
            _baseValue == other._baseValue
            && _multiplier == other._multiplier
            && _min == other._min
            && _max == other._max;

        void ISerializationCallbackReceiver.OnAfterDeserialize() {
            _value = (_baseValue * _multiplier).Clamp(_min, _max);
        }

        readonly void ISerializationCallbackReceiver.OnBeforeSerialize() {
        }
    }
}