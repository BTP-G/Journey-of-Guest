using System;
using System.Runtime.CompilerServices;

namespace Xoderony.Extensions {

    public static class DoubleExtensions {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Abs(this double value) {
            return Math.Abs(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Clamp(this double value, double min, double max) {
            return Math.Clamp(value, min, max);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Clamp01(this double value) {
            return Math.Clamp(value, 0, 1);
        }
    }
}
