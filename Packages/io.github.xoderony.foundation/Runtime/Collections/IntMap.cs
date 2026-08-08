using System;
using System.Runtime.CompilerServices;

namespace Xoderony.Collections {

    public class IntMap<TValue> {

        private const int MinEntryLength = 8;

        private int _count;

        private Entry[] _entries;

        public IntMap() {
            _entries = new Entry[MinEntryLength];
        }

        public IntMap(int capacity) {
            _entries = new Entry[GetEntryLengthForCapacity(capacity, MinEntryLength)];
        }

        public int Capacity => _entries.Length >> 1;

        public int Count => _count;

        public void Clear() {
            _count = 0;
            Array.Clear(
                _entries,
                0,
                _entries.Length
            );
        }

        public Enumerator GetEnumerator() {
            return new Enumerator(_entries);
        }

        public bool ContainsKey(int key) {
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

        public void EnsureCapacity(int capacity) {
            if (capacity <= (_entries.Length >> 1)) {
                return;
            }
            var entryLength = GetEntryLengthForCapacity(capacity, _entries.Length);
            Rebuild(entryLength);
        }

        private void Rebuild(int entryLength) {
            var oldEntries = _entries;
            var oldEntryLength = oldEntries.Length;
            var newEntries = new Entry[entryLength];
            var mask = entryLength - 1;
            for (var i = 0; i < oldEntryLength; i++) {
                ref var oldEntry = ref oldEntries[i];
                if (oldEntry.State == EntryState.Unused) {
                    continue;
                }
                var index = GetStartIndexForKey(oldEntry.Key, mask);
                ref var entry = ref newEntries[index];
                while (entry.State != EntryState.Unused) {
                    index = (index + 1) & mask;
                    entry = ref newEntries[index];
                }
                entry.State = EntryState.Used;
                entry.Key = oldEntry.Key;
                entry.Value = oldEntry.Value;
            }
            _entries = newEntries;
        }

        public bool Add(int key, in TValue value) {
            if (_count >= (_entries.Length >> 1)) {
                Rebuild(_entries.Length << 1);
            }
            var maxProbeCount = _count + 1;
            var mask = _entries.Length - 1;
            var index = GetStartIndexForKey(key, mask);
            for (var probeCount = 0; probeCount < maxProbeCount; probeCount++) {
                ref var entry = ref _entries[index];
                if (entry.State == EntryState.Unused) {
                    entry.State = EntryState.Used;
                    entry.Key = key;
                    entry.Value = value;
                    _count++;
                    return true;
                }
                if (entry.Key == key) {
                    return false;
                }
                index = (index + 1) & mask;
            }
            throw new InvalidOperationException("Probe count exceeded expected search range.");
        }

        public ref TValue GetValueRefOrAddDefault(int key, out bool exists) {
            if (_count >= (_entries.Length >> 1)) {
                Rebuild(_entries.Length << 1);
            }
            var maxProbeCount = _count + 1;
            var mask = _entries.Length - 1;
            var index = GetStartIndexForKey(key, mask);
            for (var probeCount = 0; probeCount < maxProbeCount; probeCount++) {
                ref var entry = ref _entries[index];
                if (entry.State == EntryState.Unused) {
                    entry.State = EntryState.Used;
                    entry.Key = key;
                    entry.Value = default;
                    _count++;
                    exists = false;
                    return ref entry.Value;
                }
                if (entry.Key == key) {
                    exists = true;
                    return ref entry.Value;
                }
                index = (index + 1) & mask;
            }
            throw new InvalidOperationException("Probe count exceeded expected search range.");
        }

        public ref TValue GetValueRefOrNullRef(int key, out bool exists) {
            var maxProbeCount = _count + 1;
            var mask = _entries.Length - 1;
            var index = GetStartIndexForKey(key, mask);
            for (var probeCount = 0; probeCount < maxProbeCount; probeCount++) {
                ref var entry = ref _entries[index];
                if (entry.State == EntryState.Unused) {
                    exists = false;
                    return ref Unsafe.NullRef<TValue>();
                }
                if (entry.Key == key) {
                    exists = true;
                    return ref entry.Value;
                }
                index = (index + 1) & mask;
            }
            throw new InvalidOperationException("Probe count exceeded expected search range.");
        }

        public void Set(int key, in TValue value) {
            if (_count >= (_entries.Length >> 1)) {
                Rebuild(_entries.Length << 1);
            }
            var maxProbeCount = _count + 1;
            var mask = _entries.Length - 1;
            var index = GetStartIndexForKey(key, mask);
            for (var probeCount = 0; probeCount < maxProbeCount; probeCount++) {
                ref var entry = ref _entries[index];
                if (entry.State == EntryState.Unused) {
                    entry.State = EntryState.Used;
                    entry.Key = key;
                    entry.Value = value;
                    _count++;
                    return;
                }
                if (entry.Key == key) {
                    entry.Value = value;
                    return;
                }
                index = (index + 1) & mask;
            }
            throw new InvalidOperationException("Probe count exceeded expected search range.");
        }

        public bool TryGetValue(int key, out TValue value) {
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

        private static int GetEntryLengthForCapacity(int capacity, int currentEntryLength) {
            var requiredEntryLength = capacity << 1;
            var entryLength = currentEntryLength;
            while (entryLength < requiredEntryLength) {
                entryLength <<= 1;
            }
            return entryLength;
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

        public struct Enumerator {

            private readonly Entry[] _entries;

            private int _index;

            internal Enumerator(Entry[] entries) {
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
