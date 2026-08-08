using System.Runtime.CompilerServices;

namespace Xoderony.Extensions {

    public static class StringExtensions {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrEmpty(this string value) {
            return string.IsNullOrEmpty(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrWhiteSpace(this string value) {
            return string.IsNullOrWhiteSpace(value);
        }
    }
}
