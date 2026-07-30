using System.Collections.Generic;

namespace Xoderony.ObjectPool.Generic {

    public class DictionaryPool<TKey, TValue> : GenericPool<Dictionary<TKey, TValue>> {
        public static readonly DictionaryPool<TKey, TValue> Shared = new();

        public static CollectionScope<Dictionary<TKey, TValue>, KeyValuePair<TKey, TValue>> Rent(out Dictionary<TKey, TValue> dictionary) {
            dictionary = Shared.Rent();
            return new(Shared, dictionary);
        }
    }
}