using System;
using System.Runtime.CompilerServices;

namespace Xoderony.Extensions {

    /// <summary>无符号类型绝对值恒等，故不提供 Abs。</summary>
    public static class UInt64Extensions {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong Clamp(this ulong value, ulong min, ulong max) {
            return Math.Clamp(value, min, max);
        }
    }
}
