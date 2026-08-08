using System;
using System.Runtime.CompilerServices;

namespace Xoderony.Extensions {

    public static class DecimalExtensions {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static decimal Abs(this decimal value) {
            return Math.Abs(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static decimal Clamp(this decimal value, decimal min, decimal max) {
            return Math.Clamp(value, min, max);
        }
    }
}
