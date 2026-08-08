using System;
using System.Collections.Generic;

namespace Xoderony.Collections {

    public ref struct SpanList<T> {

        private Span<T> _fullSpan;

        private int _count;

        private ComparerDelegate<T> _comparer;

        public SpanList(Span<T> span, int count = 0, ComparerDelegate<T> comparer = null) {
            if ((uint)count > (uint)span.Length) {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
            _fullSpan = span;
            _count = count;
            _comparer = comparer;
        }

        public readonly ref T this[int index] => ref ElementAt(index);

        public readonly int Count => _count;

        public readonly int Capacity => _fullSpan.Length;

        public readonly int RemainingCapacity => _fullSpan.Length - _count;

        public readonly bool IsFull => _count >= _fullSpan.Length;

        public ComparerDelegate<T> Comparer {
            readonly get => _comparer;
            set => _comparer = value;
        }

        public readonly Span<T> FullSpan => _fullSpan;

        public readonly Span<T> Span => _fullSpan[.._count];

        public readonly ref T ElementAt(int index) {
            if ((uint)index >= (uint)_count) {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            return ref _fullSpan[index];
        }

        public readonly Span<T>.Enumerator GetEnumerator() {
            return Span.GetEnumerator();
        }

        public readonly T[] ToArray() {
            return Span.ToArray();
        }

        public void Clear() {
            _count = 0;
        }

        public bool Add(in T item) {
            if (IsFull) {
                return false;
            }
            _fullSpan[_count] = item;
            _count++;
            return true;
        }

        public bool AddRange(ReadOnlySpan<T> items) {
            if (items.Length > RemainingCapacity) {
                return false;
            }
            var destination = _fullSpan.Slice(_count, items.Length);
            items.CopyTo(destination);
            _count += items.Length;
            return true;
        }

        public bool Insert(int index, in T item) {
            if ((uint)index > (uint)_count) {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            if (IsFull) {
                return false;
            }
            var countToMove = _count - index;
            var fullSpan = _fullSpan;
            if (countToMove > 0) {
                var source = fullSpan.Slice(index, countToMove);
                var destination = fullSpan.Slice(index + 1, countToMove);
                source.CopyTo(destination);
            }
            fullSpan[index] = item;
            _count++;
            return true;
        }

        public bool SwapInsert(int index, in T item) {
            if ((uint)index > (uint)_count) {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            if (IsFull) {
                return false;
            }
            if (index < _count) {
                _fullSpan[_count] = _fullSpan[index];
            }
            _fullSpan[index] = item;
            _count++;
            return true;
        }

        public bool Remove(in T item) {
            var index = IndexOf(item);
            if (index == -1) {
                return false;
            }
            RemoveAt(index);
            return true;
        }

        public void RemoveAt(int index) {
            if ((uint)index >= (uint)_count) {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            var span = Span;
            var countToMove = _count - index - 1;
            if (countToMove > 0) {
                var source = span.Slice(index + 1, countToMove);
                var destination = span.Slice(index, countToMove);
                source.CopyTo(destination);
            }
            _count--;
        }

        public bool SwapRemove(in T item) {
            var index = IndexOf(item);
            if (index == -1) {
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
                _fullSpan[index] = _fullSpan[lastIndex];
            }
            _count--;
        }

        public int RemoveAll(Predicate<T> match) {
            if (match == null) {
                throw new ArgumentNullException(nameof(match));
            }
            var span = Span;
            var write = 0;
            var read = 0;
            for (; read < _count; read++) {
                if (match(span[read])) {
                    continue;
                }
                span[write++] = span[read];
            }
            _count = write;
            return read - write;
        }

        public readonly int IndexOf(in T item) {
            var span = Span;
            if (_comparer is not null) {
                for (var i = 0; i < span.Length; i++) {
                    if (_comparer(span[i], item)) {
                        return i;
                    }
                }
            } else {
                var comparer = EqualityComparer<T>.Default;
                for (var i = 0; i < span.Length; i++) {
                    if (comparer.Equals(span[i], item)) {
                        return i;
                    }
                }
            }
            return -1;
        }

        public readonly bool Contains(in T item) {
            return IndexOf(item) != -1;
        }

        public static implicit operator SpanList<T>(Span<T> span) {
            return new SpanList<T>(span);
        }

        public static implicit operator Span<T>(SpanList<T> spanList) {
            return spanList._fullSpan[..spanList._count];
        }
    }

    public delegate bool ComparerDelegate<T>(in T a, in T b);

}
