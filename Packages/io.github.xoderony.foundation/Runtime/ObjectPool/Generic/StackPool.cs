using System.Collections.Generic;

namespace Xoderony.ObjectPool.Generic {

    public class StackPool<T> : GenericPool<Stack<T>> {
        public static readonly StackPool<T> Shared = new();
    }
}