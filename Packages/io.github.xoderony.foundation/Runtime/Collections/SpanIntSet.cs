using System;

namespace Xoderony.Collections {

    public ref struct SpanIntSet {

        private Span<Entry> _entries;

        private int _count;

        public SpanIntSet(Span<Entry> buffer) {
            if (buffer.Length <= 0) {
                throw new ArgumentException("Buffer length must be greater than zero.", nameof(buffer));
            }
            if ((buffer.Length & (buffer.Length - 1)) != 0) {
                throw new ArgumentException("Buffer length must be a power of two.", nameof(buffer));
            }
            _entries = buffer;
            _count = 0;
            _entries.Fill(default);
        }

        public readonly int Count => _count;

        public readonly int Capacity => _entries.Length >> 1;

        public readonly int BufferLength => _entries.Length;

        public readonly bool IsFull => _count >= Capacity;

        public readonly int RemainingCapacity => Capacity - _count;

        public readonly Span<Entry> FullSpan => _entries;

        public static int GetBufferLengthForCapacity(int capacity) {
            if (capacity <= 0) {
                throw new ArgumentException("Capacity must be greater than zero.", nameof(capacity));
            }
            if (capacity > 5120) {
                throw new ArgumentException("Capacity must be less than or equal to 5120.", nameof(capacity));
            }
            var requiredBufferLength = capacity * 2;
            var bufferLength = 1;
            while (bufferLength < requiredBufferLength) {
                bufferLength <<= 1;
            }
            return bufferLength;
        }

        public AddStatus Add(int item) {
            if (IsFull) {
                return AddStatus.Full;
            }
            var entries = _entries;
            var maxProbeCount = _count + 1;
            var mask = entries.Length - 1;
            var index = GetStartIndexForItem(item, mask);
            for (var probeCount = 0; probeCount < maxProbeCount; probeCount++) {
                ref var entry = ref entries[index];
                if (entry.State == EntryState.Unused) {
                    entry.Value = item;
                    entry.State = EntryState.Used;
                    _count++;
                    return AddStatus.Added;
                }
                if (entry.Value == item) {
                    return AddStatus.Duplicate;
                }
                index = (index + 1) & mask;
            }
            throw new InvalidOperationException("Probe count exceeded expected search range.");
        }

        public readonly bool Contains(int item) {
            var entries = _entries;
            var maxProbeCount = _count + 1;
            var mask = entries.Length - 1;
            var index = GetStartIndexForItem(item, mask);
            for (var probeCount = 0; probeCount < maxProbeCount; probeCount++) {
                ref var entry = ref entries[index];
                if (entry.State == EntryState.Unused) {
                    return false;
                }
                if (entry.Value == item) {
                    return true;
                }
                index = (index + 1) & mask;
            }
            throw new InvalidOperationException("Probe count exceeded expected search range.");
        }

        public void Clear() {
            _count = 0;
            _entries.Fill(default);
        }

        public readonly Enumerator GetEnumerator() {
            return new Enumerator(_entries);
        }

        private static int GetStartIndexForItem(int item, int mask) {
            var hash = (uint)item;
            hash ^= hash >> 16;
            hash *= 0x7feb352d;
            hash ^= hash >> 15;
            return ((int)hash) & mask;
        }

        public struct Entry {

            public int Value;

            internal EntryState State;

        }

        public ref struct Enumerator {

            private readonly Span<Entry> _entries;

            private int _index;

            internal Enumerator(Span<Entry> entries) {
                _entries = entries;
                _index = -1;
            }

            public readonly int Current => _entries[_index].Value;

            public bool MoveNext() {
                for (_index++; _index < _entries.Length; _index++) {
                    if (_entries[_index].State == EntryState.Used) {
                        return true;
                    }
                }
                return false;
            }
        }
    }
}
