using System.Collections.Generic;

namespace Xoderony.ObjectPool.Generic {

    public class QueuePool<T> : GenericPool<Queue<T>> {
        public static readonly QueuePool<T> Shared = new();
    }
}