using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Xoderony.Extensions {

    public static class CollectionExtensions {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrEmpty<T>(this ICollection<T> collection) {
            return (collection is null) || (collection.Count is 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrEmpty<T>(this T collection) where T : ICollection {
            return (collection is null) || (collection.Count is 0);
        }
    }
}
