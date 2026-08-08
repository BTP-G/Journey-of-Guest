using System;
using System.Runtime.CompilerServices;

namespace Xoderony.Extensions {

    public static class Int32Extensions {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Abs(this int value) {
            return Math.Abs(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Clamp(this int value, int min, int max) {
            return Math.Clamp(
                value,
                min,
                max
            );
        }
    }
}
