using System.Collections.Generic;

namespace Xoderony.ObjectPool.Generic {

    public class ListPool<T> : GenericPool<List<T>> {
        public static readonly ListPool<T> Shared = new();

        public static CollectionScope<List<T>, T> Rent(out List<T> list) {
            list = Shared.Rent();
            return new(Shared, list);
        }
    }
}