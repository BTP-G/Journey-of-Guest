using System;
using System.Runtime.CompilerServices;

namespace Xoderony.Extensions {

    public static class Int64Extensions {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long Abs(this long value) {
            return Math.Abs(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long Clamp(this long value, long min, long max) {
            return Math.Clamp(value, min, max);
        }
    }
}
