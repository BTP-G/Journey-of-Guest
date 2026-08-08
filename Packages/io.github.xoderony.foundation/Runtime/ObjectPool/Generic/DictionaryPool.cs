using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Xoderony.ObjectPool.Generic {

    /// <summary>Dictionary 池：归还时自动清空元素。</summary>
    public class DictionaryPool<TKey, TValue> : CollectionPool<Dictionary<TKey, TValue>, KeyValuePair<TKey, TValue>> {

        public static readonly DictionaryPool<TKey, TValue> Shared = new();

        /// <summary>租借一个空 Dictionary；作用域 Dispose 后不得继续使用。</summary>
        /// <param name="dictionary">租借到的 Dictionary。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PooledObjectScope<Dictionary<TKey, TValue>> Rent(out Dictionary<TKey, TValue> dictionary) {
            dictionary = Shared.Rent();
            return new(Shared, dictionary);
        }
    }
}
