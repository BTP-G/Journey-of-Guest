using System;
using System.Runtime.CompilerServices;

namespace Xoderony.Extensions {

    public static class SByteExtensions {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Abs(this sbyte value) {
            return Math.Abs(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static sbyte Clamp(this sbyte value, sbyte min, sbyte max) {
            return Math.Clamp(value, min, max);
        }
    }
}
