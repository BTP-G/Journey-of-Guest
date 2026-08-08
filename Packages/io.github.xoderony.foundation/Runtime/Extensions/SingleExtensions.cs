using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Xoderony.Extensions {

    public static class SingleExtensions {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Abs(this float value) {
            return Math.Abs(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Clamp(this float value, float min, float max) {
            return Math.Clamp(
                value,
                min,
                max
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Clamp01(this float value) {
            return Math.Clamp(
                value,
                0,
                1
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float LerpTo(this float from, float to, float t) {
            return from + ((to - from) * t.Clamp01());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float LerpToUnclamped(this float from, float to, float t) {
            return from + ((to - from) * t);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float MoveTowards(this float from, float to, float maxDelta) {
            Debug.Assert(maxDelta >= 0f);
            var delta = to - from;
            if (delta > maxDelta) {
                return from + maxDelta;
            }
            if (delta < (-maxDelta)) {
                return from - maxDelta;
            }
            return to;
        }
    }
}
