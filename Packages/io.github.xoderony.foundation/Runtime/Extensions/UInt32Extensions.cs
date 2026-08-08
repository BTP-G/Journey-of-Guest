using System;
using System.Runtime.CompilerServices;

namespace Xoderony.Extensions {

    /// <summary>无符号类型绝对值恒等，故不提供 Abs。</summary>
    public static class UInt32Extensions {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Clamp(this uint value, uint min, uint max) {
            return Math.Clamp(value, min, max);
        }
    }
}
