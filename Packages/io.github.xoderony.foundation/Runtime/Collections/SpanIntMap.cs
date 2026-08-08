using System;
using System.Runtime.CompilerServices;

namespace Xoderony.Collections {

    public ref struct SpanIntMap<TValue> {

        private Span<Entry> _entries;

        private int _count;

        public SpanIntMap(Span<Entry> buffer) {
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

        public readonly int RemainingCapacity => Capacity - _count;

        public readonly bool IsFull => _count >= Capacity;

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

        public AddStatus Add(int key, TValue value) {
            if (IsFull) {
                return AddStatus.Full;
            }
            var maxProbeCount = _count + 1;
            var mask = _entries.Length - 1;
            var index = GetStartIndexForKey(key, mask);
            for (var probeCount = 0; probeCount < maxProbeCount; probeCount++) {
                ref var entry = ref _entries[index];
                if (entry.State == EntryState.Unused) {
                    entry.Key = key;
                    entry.Value = value;
                    entry.State = EntryState.Used;
                    _count++;
                    return AddStatus.Added;
                }
                if (entry.Key == key) {
                    return AddStatus.Duplicate;
                }
                index = (index + 1) & mask;
            }
            throw new InvalidOperationException("Probe count exceeded expected search range.");
        }

        public SetStatus Set(int key, TValue value) {
            var maxProbeCount = _count + 1;
            var mask = _entries.Length - 1;
            var index = GetStartIndexForKey(key, mask);
            for (var probeCount = 0; probeCount < maxProbeCount; probeCount++) {
                ref var entry = ref _entries[index];
                if (entry.State == EntryState.Unused) {
                    if (IsFull) {
                        return SetStatus.Full;
                    }
                    entry.Key = key;
                    entry.Value = value;
                    entry.State = EntryState.Used;
                    _count++;
                    return SetStatus.Added;
                }
                if (entry.Key == key) {
                    entry.Value = value;
                    return SetStatus.Updated;
                }
                index = (index + 1) & mask;
            }
            throw new InvalidOperationException("Probe count exceeded expected search range.");
        }

        public readonly bool ContainsKey(int key) {
            var maxProbeCount = _count + 1;
            var mask = _entries.Length - 1;
            var index = GetStartIndexForKey(key, mask);
            for (var probeCount = 0; probeCount < maxProbeCount; probeCount++) {
                ref var entry = ref _entries[index];
                if (entry.State == EntryState.Unused) {
                    return false;
                }
                if (entry.Key == key) {
                    return true;
                }
                index = (index + 1) & mask;
            }
            throw new InvalidOperationException("Probe count exceeded expected search range.");
        }

        public readonly bool TryGetValue(int key, out TValue value) {
            var maxProbeCount = _count + 1;
            var mask = _entries.Length - 1;
            var index = GetStartIndexForKey(key, mask);
            for (var probeCount = 0; probeCount < maxProbeCount; probeCount++) {
                ref var entry = ref _entries[index];
                if (entry.State == EntryState.Unused) {
                    value = default;
                    return false;
                }
                if (entry.Key == key) {
                    value = entry.Value;
                    return true;
                }
                index = (index + 1) & mask;
            }
            throw new InvalidOperationException("Probe count exceeded expected search range.");
        }

        public ref TValue GetValueRefOrAddDefault(int key, out GetOrAddStatus status) {
            var maxProbeCount = _count + 1;
            var mask = _entries.Length - 1;
            var index = GetStartIndexForKey(key, mask);
            for (var probeCount = 0; probeCount < maxProbeCount; probeCount++) {
                ref var entry = ref _entries[index];
                if (entry.State == EntryState.Unused) {
                    if (IsFull) {
                        status = GetOrAddStatus.Full;
                        return ref Unsafe.NullRef<TValue>();
                    }
                    entry.Key = key;
                    entry.Value = default;
                    entry.State = EntryState.Used;
                    _count++;
                    status = GetOrAddStatus.Added;
                    return ref entry.Value;
                }
                if (entry.Key == key) {
                    status = GetOrAddStatus.Found;
                    return ref entry.Value;
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

        private static int GetStartIndexForKey(int key, int mask) {
            var hash = (uint)key;
            hash ^= hash >> 16;
            hash *= 0x7feb352d;
            hash ^= hash >> 15;
            return ((int)hash) & mask;
        }

        public struct Entry {

            public int Key;

            public TValue Value;

            internal EntryState State;

        }

        public ref struct Enumerator {

            private readonly Span<Entry> _entries;

            private int _index;

            internal Enumerator(Span<Entry> entries) {
                _entries = entries;
                _index = -1;
            }

            public readonly ref readonly Entry Current => ref _entries[_index];

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
