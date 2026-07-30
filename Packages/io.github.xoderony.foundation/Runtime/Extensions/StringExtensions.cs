using System.Runtime.CompilerServices;
using System.Text;

namespace Xoderony.Extensions {

    public static class StringExtensions {

        private static readonly StringBuilder _sb = new(512);

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
