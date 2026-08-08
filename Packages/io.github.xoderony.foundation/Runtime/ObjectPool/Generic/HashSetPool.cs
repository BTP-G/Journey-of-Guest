using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Xoderony.ObjectPool.Generic {

    /// <summary>HashSet 池：归还时自动清空元素。</summary>
    public class HashSetPool<T> : CollectionPool<HashSet<T>, T> {

        public static readonly HashSetPool<T> Shared = new();

        /// <summary>租借一个空 HashSet；作用域 Dispose 后不得继续使用。</summary>
        /// <param name="set">租借到的 HashSet。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PooledObjectScope<HashSet<T>> Rent(out HashSet<T> set) {
            set = Shared.Rent();
            return new(Shared, set);
        }
    }
}
