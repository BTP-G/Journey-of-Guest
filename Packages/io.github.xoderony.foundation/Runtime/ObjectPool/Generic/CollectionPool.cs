using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Xoderony.ObjectPool.Generic {

    /// <summary>List、HashSet、Dictionary 等 ICollection 集合共用对象池：归还时清空元素后再缓存。</summary>
    /// <typeparam name="TCollection">被池化的集合类型，需实现 <see cref="ICollection{TElement}"/> 且可无参构造。</typeparam>
    /// <typeparam name="TElement">集合元素类型。</typeparam>
    public class CollectionPool<TCollection, TElement> : IPool<TCollection> where TCollection : class, ICollection<TElement>, new() {
        private readonly ushort _capacity;
        private readonly Stack<TCollection> _pool;

        /// <summary>创建容量固定的集合池；池满后归还的对象直接丢弃。</summary>
        public CollectionPool(ushort capacity = 16) {
            _capacity = capacity;
            _pool = new(capacity);
        }

        public ushort Capacity {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _capacity;
        }

        /// <summary>租借一个空集合；池空时新建。</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TCollection Rent() {
            return _pool.TryPop(out var result) ? result : new();
        }

        /// <summary>清空并归还集合；池满或值为 null 时直接丢弃。</summary>
        /// <param name="value">待归还的集合，归还后调用方不得继续使用。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(TCollection value) {
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
    }
}
