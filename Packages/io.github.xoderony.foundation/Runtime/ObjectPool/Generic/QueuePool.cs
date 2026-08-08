using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Xoderony.ObjectPool.Generic {

    /// <summary>Queue 池：归还时自动清空元素；Queue 不实现 <see cref="ICollection{T}"/>，因此单独实现。</summary>
    public class QueuePool<T> : IPool<Queue<T>> {
        private readonly ushort _capacity;
        private readonly Stack<Queue<T>> _pool;

        /// <summary>创建容量固定的 Queue 池；池满后归还的对象直接丢弃。</summary>
        public QueuePool(ushort capacity = 16) {
            _capacity = capacity;
            _pool = new(capacity);
        }

        public static readonly QueuePool<T> Shared = new();

        public ushort Capacity {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _capacity;
        }

        /// <summary>租借一个空 Queue；池空时新建。</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Queue<T> Rent() {
            return _pool.TryPop(out var result) ? result : new();
        }

        /// <summary>清空并归还 Queue；池满或值为 null 时直接丢弃。</summary>
        /// <param name="value">待归还的 Queue，归还后调用方不得继续使用。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(Queue<T> value) {
            Debug.Assert(value is not null);
            if (value is null || _pool.Count >= _capacity) {
                return;
            }
            value.Clear();
            _pool.Push(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() {
            _pool.Clear();
        }

        /// <summary>租借一个空 Queue；作用域 Dispose 后不得继续使用。</summary>
        /// <param name="queue">租借到的 Queue。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PooledObjectScope<Queue<T>> Rent(out Queue<T> queue) {
            queue = Shared.Rent();
            return new(Shared, queue);
        }
    }
}
