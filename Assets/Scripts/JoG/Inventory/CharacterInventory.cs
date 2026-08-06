using JoG.Item;
using System;
using System.Collections.Generic;

namespace JoG.Inventory {

    [Serializable]
    public sealed class CharacterInventory : IComponent {

        private readonly Dictionary<ItemData, int> _itemToCount = new();

        public int Count => _itemToCount.Count;

        public event Action<ItemData, int> ItemCountChanged;

        public Dictionary<ItemData, int>.Enumerator GetEnumerator() {
            return _itemToCount.GetEnumerator();
        }

        public int GetItemCount(ItemData item) {
            if (item == null) {
                throw new ArgumentNullException(nameof(item));
            }
            return _itemToCount.TryGetValue(item, out var count) ? count : 0;
        }

        public bool HasEnoughItems(ItemData item, int requiredCount) {
            if (requiredCount < 0) {
                throw new ArgumentOutOfRangeException(nameof(requiredCount), requiredCount, "Required count must not be negative.");
            }
            return GetItemCount(item) >= requiredCount;
        }

        public void AddItem(ItemData item, int count = 1) {
            if (count <= 0) {
                throw new ArgumentOutOfRangeException(nameof(count), count, "Item count must be greater than zero.");
            }
            SetItemCount(item, GetItemCount(item) + count);
        }

        public bool RemoveItem(ItemData item, int count = 1) {
            if (count <= 0) {
                throw new ArgumentOutOfRangeException(nameof(count), count, "Item count must be greater than zero.");
            }

            var currentCount = GetItemCount(item);
            if (currentCount < count) {
                return false;
            }

            SetItemCount(item, currentCount - count);
            return true;
        }

        public void SetItemCount(ItemData item, int count) {
            if (item == null) {
                throw new ArgumentNullException(nameof(item));
            }
            if (count < 0) {
                throw new ArgumentOutOfRangeException(nameof(count), count, "Item count must not be negative.");
            }

            var currentCount = GetItemCount(item);
            if (currentCount == count) {
                return;
            }

            if (count == 0) {
                _itemToCount.Remove(item);
            } else {
                _itemToCount[item] = count;
            }
            ItemCountChanged?.Invoke(item, count);
        }

        public void Clear() {
            while (_itemToCount.Count > 0) {
                var enumerator = _itemToCount.GetEnumerator();
                enumerator.MoveNext();
                var item = enumerator.Current.Key;
                enumerator.Dispose();
                _itemToCount.Remove(item);
                ItemCountChanged?.Invoke(item, 0);
            }
        }

        internal void ClearSilently() {
            _itemToCount.Clear();
        }
    }
}

