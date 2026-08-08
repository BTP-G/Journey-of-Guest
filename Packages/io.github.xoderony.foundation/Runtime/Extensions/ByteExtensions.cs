using System;
using System.Runtime.CompilerServices;

namespace Xoderony.Extensions {

    /// <summary>无符号类型绝对值恒等，故不提供 Abs。</summary>
    public static class ByteExtensions {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte Clamp(this byte value, byte min, byte max) {
            return Math.Clamp(value, min, max);
        }
    }
}
