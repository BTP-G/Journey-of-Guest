using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Xoderony.Numerics {

    [Serializable]
    public struct Q16 : IEquatable<Q16>, IComparable<Q16> {

        [SerializeField]
        private int _rawValue;

        private const int FractionalBitCount = 16;

        private const int Value2Raw = 1 << FractionalBitCount;

        private const float Raw2Value = 1f / Value2Raw;

        public static Q16 Zero => default;

        public static Q16 One => FromRawValue(Value2Raw);

        private Q16(int rawValue) {
            _rawValue = rawValue;
        }

        public Q16(int numerator, int denominator) {
            _rawValue = ClampToInt32((((long)numerator) * Value2Raw) / denominator);
        }

        public Q16(float value) {
            _rawValue = ClampToInt32(value * Value2Raw);
        }

        public readonly int RawValue => _rawValue;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Q16 FromRawValue(int rawValue) {
            return new Q16(rawValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int Multiply(int value) {
            return ClampToInt32((((long)value) * _rawValue) >> FractionalBitCount);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly long Multiply(long value) {
            return (value * _rawValue) >> FractionalBitCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float ToFloat() {
            return _rawValue * Raw2Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int ToInt32() {
            return _rawValue >> FractionalBitCount;
        }

        public override readonly string ToString() {
            return ToFloat().ToString();
        }

        public override readonly bool Equals(object obj) {
            return obj is Q16 other && Equals(other);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(Q16 other) {
            return _rawValue == other._rawValue;
        }

        public override readonly int GetHashCode() {
            return _rawValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int CompareTo(Q16 other) {
            return _rawValue.CompareTo(other._rawValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int ClampToInt32(long value) {
            if (value >= int.MaxValue) {
                return int.MaxValue;
            }
            if (value <= int.MinValue) {
                return int.MinValue;
            }
            return (int)value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int ClampToInt32(float value) {
            if (value >= int.MaxValue) {
                return int.MaxValue;
            }
            if (value <= int.MinValue) {
                return int.MinValue;
            }
            return (int)value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator *(int value, Q16 valueScale) {
            return valueScale.Multiply(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int operator *(Q16 valueScale, int value) {
            return valueScale.Multiply(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long operator *(long value, Q16 valueScale) {
            return valueScale.Multiply(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long operator *(Q16 valueScale, long value) {
            return valueScale.Multiply(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Q16 operator +(Q16 left, Q16 right) {
            var rawValue = ClampToInt32(((long)left._rawValue) + right._rawValue);
            return FromRawValue(rawValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Q16 operator *(Q16 left, Q16 right) {
            var rawValue = ClampToInt32((((long)left._rawValue) * right._rawValue) >> FractionalBitCount);
            return FromRawValue(rawValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Q16 left, Q16 right) {
            return left._rawValue == right._rawValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Q16 left, Q16 right) {
            return left._rawValue != right._rawValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(Q16 left, Q16 right) {
            return left._rawValue < right._rawValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(Q16 left, Q16 right) {
            return left._rawValue > right._rawValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(Q16 left, Q16 right) {
            return left._rawValue <= right._rawValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(Q16 left, Q16 right) {
            return left._rawValue >= right._rawValue;
        }

    }

}
