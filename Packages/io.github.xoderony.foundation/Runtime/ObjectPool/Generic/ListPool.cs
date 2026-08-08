using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Xoderony.ObjectPool.Generic {

    /// <summary>List 池：归还时自动清空元素。</summary>
    public class ListPool<T> : CollectionPool<List<T>, T> {

        public static readonly ListPool<T> Shared = new();

        /// <summary>租借一个空 List；作用域 Dispose 后不得继续使用。</summary>
        /// <param name="list">租借到的 List。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PooledObjectScope<List<T>> Rent(out List<T> list) {
            list = Shared.Rent();
            return new(Shared, list);
        }
    }
}
