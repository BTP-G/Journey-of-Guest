using System;
using System.Runtime.CompilerServices;

namespace Xoderony.Extensions {

    public static class Int16Extensions {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Abs(this short value) {
            return Math.Abs(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short Clamp(this short value, short min, short max) {
            return Math.Clamp(value, min, max);
        }
    }
}
