using System.Collections.Generic;

namespace Xoderony.ObjectPool.Generic {
    public class HashSetPool<T> : GenericPool<HashSet<T>> {
        public static readonly HashSetPool<T> Shared = new();

        public static CollectionScope<HashSet<T>, T> Rent(out HashSet<T> set) { 
            set = Shared.Rent();
            return new(Shared, set);
        }
    }
}
