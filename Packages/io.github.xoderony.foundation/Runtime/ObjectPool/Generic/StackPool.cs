using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Xoderony.ObjectPool.Generic {

    /// <summary>Stack 池：归还时自动清空元素；Stack 不实现 <see cref="ICollection{T}"/>，因此单独实现。</summary>
    public class StackPool<T> : IPool<Stack<T>> {
        private readonly ushort _capacity;
        private readonly Stack<Stack<T>> _pool;

        /// <summary>创建容量固定的 Stack 池；池满后归还的对象直接丢弃。</summary>
        public StackPool(ushort capacity = 16) {
            _capacity = capacity;
            _pool = new(capacity);
        }

        public static readonly StackPool<T> Shared = new();

        public ushort Capacity {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _capacity;
        }

        /// <summary>租借一个空 Stack；池空时新建。</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Stack<T> Rent() {
            return _pool.TryPop(out var result) ? result : new();
        }

        /// <summary>清空并归还 Stack；池满或值为 null 时直接丢弃。</summary>
        /// <param name="value">待归还的 Stack，归还后调用方不得继续使用。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(Stack<T> value) {
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

        /// <summary>租借一个空 Stack；作用域 Dispose 后不得继续使用。</summary>
        /// <param name="stack">租借到的 Stack。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PooledObjectScope<Stack<T>> Rent(out Stack<T> stack) {
            stack = Shared.Rent();
            return new(Shared, stack);
        }
    }
}
