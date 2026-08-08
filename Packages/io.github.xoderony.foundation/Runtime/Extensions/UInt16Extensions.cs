using System;
using System.Runtime.CompilerServices;

namespace Xoderony.Extensions {

    /// <summary>无符号类型绝对值恒等，故不提供 Abs。</summary>
    public static class UInt16Extensions {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort Clamp(this ushort value, ushort min, ushort max) {
            return Math.Clamp(value, min, max);
        }
    }
}
