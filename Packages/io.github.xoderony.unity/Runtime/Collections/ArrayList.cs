using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Xoderony.Collections {

    [Serializable]
    public class ArrayList<T> : ISerializationCallbackReceiver {

        [SerializeField]
        private T[] _items;

        [NonSerialized]
        private int _count;

        [NonSerialized]
        private ComparerDelegate<T> _comparer;

        public ArrayList() {
            _items = Array.Empty<T>();
        }

        public ArrayList(int capacity) {
            _items = new T[capacity];
        }

        public ArrayList(ReadOnlySpan<T> items) {
            _items = items.ToArray();
            _count = items.Length;
        }

        public ref T this[int index] => ref ElementAt(index);

        public int Count => _count;

        public int Capacity => _items.Length;

        public ComparerDelegate<T> Comparer {
            get => _comparer;
            set => _comparer = value;
        }

        public Span<T> FullSpan => _items;

        public Span<T> Span => new Span<T>(_items, 0, _count);

        public ReadOnlySpan<T> ReadOnlySpan => new ReadOnlySpan<T>(_items, 0, _count);

        void ISerializationCallbackReceiver.OnBeforeSerialize() {
            _items ??= Array.Empty<T>();
            if ((uint)_count > (uint)_items.Length) {
                _count = _items.Length;
            }
            if (_count != _items.Length) {
                Array.Resize(ref _items, _count);
            }
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize() {
            _items ??= Array.Empty<T>();
            _count = _items.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T ElementAt(int index) {
            if ((uint)index >= (uint)_count) {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            return ref _items[index];
        }

        public Span<T>.Enumerator GetEnumerator() {
            return Span.GetEnumerator();
        }

        public T[] ToArray() {
            if (_count == 0) {
                return Array.Empty<T>();
            }
            var array = new T[_count];
            Array.Copy(_items, array, _count);
            return array;
        }

        public void Clear() {
            Array.Clear(_items, 0, _count);
            _count = 0;
        }

        public void Add(in T item) {
            EnsureCapacity(_count + 1);
            _items[_count] = item;
            _count++;
        }

        public void AddRange<TCollection>(TCollection collection) where TCollection : ICollection<T> {
            var collectionCount = collection.Count;
            EnsureCapacity(_count + collectionCount);
            collection.CopyTo(_items, _count);
            _count += collectionCount;
        }

        public void AddRange(ReadOnlySpan<T> items) {
            EnsureCapacity(_count + items.Length);
            items.CopyTo(_items.AsSpan(_count));
            _count += items.Length;
        }

        public void Insert(int index, in T item) {
            if ((uint)index > (uint)_count) {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            EnsureCapacity(_count + 1);
            var countToMove = _count - index;
            Array.Copy(_items, index, _items, index + 1, countToMove);
            _items[index] = item;
            _count++;
        }

        public void SwapInsert(int index, in T item) {
            if ((uint)index > (uint)_count) {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            EnsureCapacity(_count + 1);
            if (index < _count) {
                _items[_count] = _items[index];
            }
            _items[index] = item;
            _count++;
        }

        public bool Remove(in T item) {
            var index = IndexOf(item);
            if (index < 0) {
                return false;
            }
            RemoveAt(index);
            return true;
        }

        public void RemoveAt(int index) {
            if ((uint)index >= (uint)_count) {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            _count--;
            var countToMove = _count - index;
            Array.Copy(_items, index + 1, _items, index, countToMove);
            _items[_count] = default;
        }

        public bool SwapRemove(in T item) {
            var index = IndexOf(item);
            if (index < 0) {
                return false;
            }
            SwapRemoveAt(index);
            return true;
        }

        public void SwapRemoveAt(int index) {
            if ((uint)index >= (uint)_count) {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            var lastIndex = _count - 1;
            if (index != lastIndex) {
                _items[index] = _items[lastIndex];
            }
            _count--;
            _items[_count] = default;
        }

        public int RemoveAll(Predicate<T> match) {
            if (match == null) {
                throw new ArgumentNullException(nameof(match));
            }
            var writeIndex = 0;
            for (var readIndex = 0; readIndex < _count; readIndex++) {
                var item = _items[readIndex];
                if (match(item)) {
                    continue;
                }
                _items[writeIndex] = item;
                writeIndex++;
            }
            var removedCount = _count - writeIndex;
            Array.Clear(_items, writeIndex, removedCount);
            _count = writeIndex;
            return removedCount;
        }

        public int IndexOf(in T item) {
            if (_comparer is not null) {
                for (var i = 0; i < _count; i++) {
                    if (_comparer(_items[i], item)) {
                        return i;
                    }
                }
                return -1;
            } else {
                var comparer = EqualityComparer<T>.Default;
                for (var i = 0; i < _count; i++) {
                    if (comparer.Equals(_items[i], item)) {
                        return i;
                    }
                }
            }
            return -1;
        }

        public bool Contains(in T item) {
            return IndexOf(item) >= 0;
        }

        private void EnsureCapacity(int capacity) {
            if (capacity <= _items.Length) {
                return;
            }
            var newCapacity = Math.Max(_items.Length * 2, capacity);
            var newItems = new T[newCapacity];
            Array.Copy(_items, newItems, _count);
            _items = newItems;
        }
    }
}
