using GuestUnion;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace JoG.InventorySystem {

    [Serializable]
    public class Inventory {
        private InventoryItem[] _items;
        private List<Action<int>> _itemChangedHandlers;

        public event Action<int> OnItemChanged {
            add => _itemChangedHandlers.Add(value);
            remove => _itemChangedHandlers.Add(value);
        }

        public Inventory(int size) {
            _items = new InventoryItem[size];
            for (var i = 0; i < size; ++i) {
                _items[i] = new InventoryItem(this, i);
            }
        }

        public InventoryItem this[int index] {
            get => _items[index];
            set => _items[index] = value;
        }

        public static Inventory FromJson(string json) {
            if (json.IsNullOrEmpty()) {
                return null;
            }
            var inventoryItemDatas = JsonConvert.DeserializeObject<InventoryItemData[]>(json);
            if (inventoryItemDatas.IsNullOrEmpty()) {
                return null;
            }
            var inventory = new Inventory(inventoryItemDatas.Length);
            for (var i = 0; i < inventoryItemDatas.Length; ++i) {
                var inventoryItemData = inventoryItemDatas[i];
                ItemCollector.Instance.TryGetItemDef(inventoryItemData.itemNameToken, out var itemData);
                inventory._items[i].SetDataAndCount(itemData, inventoryItemData.itemCount);
            }
            return inventory;
        }

        public void PublishItemChanged(int index) {
            foreach (var handler in _itemChangedHandlers.AsSpan()) {
                handler.Invoke(index);
            }
        }

        public InventoryItem GetItemSafe(int index) {
            return (index < 0 || index >= _items.Length) ? default : _items[index];
        }

        public void ExchangeItemSafe(int fromIndex, int toIndex) {
            if (fromIndex == toIndex || fromIndex < 0 || fromIndex >= _items.Length || toIndex < 0 || toIndex >= _items.Length) {
                return;
            }
            (_items[toIndex], _items[fromIndex]) = (_items[fromIndex], _items[toIndex]);
            PublishItemChanged(fromIndex);
            PublishItemChanged(toIndex);
        }

        public void ExchangeItem(int fromIndex, int toIndex) {
            if (fromIndex != toIndex) {
                (_items[fromIndex], _items[toIndex]) = (_items[toIndex], _items[fromIndex]);
                PublishItemChanged(fromIndex);
                PublishItemChanged(toIndex);
            }
        }

        public void SetItemCount(ItemData itemData, byte count) {
            if (itemData is null) return;
            foreach (ref var item in new Span<InventoryItem>(_items)) {
                if (item.Data == itemData) {
                    item.Count = count;
                    return;
                }
            }
        }

        public void RemoveItem(int index) => _items[index].ResetDataAndCount();

        /// <summary>添加物品到背包中，如果背包中已存在该物品，则增加数量；如果背包中没有空位，则不添加。</summary>
        /// <param name="itemData">要添加的物品</param>
        /// <param name="count">要添加的数量</param>
        /// <returns>添加物品在背包中的索引，如果背包已满或者添加数量为0或者物品数据为null，则返回-1</returns>
        public int AddItem(ItemData itemData, byte count) {
            if (itemData is null || count == 0) {
                return -1;
            }
            var span = new Span<InventoryItem>(_items);
            var firstEmptyIndex = -1;
            for (var i = 0; i < span.Length; ++i) {
                var item = span[i];
                if (item.Data == itemData) {
                    item.Count += count;
                    return i;
                }
                if (firstEmptyIndex == -1 && item.Data is null) {
                    firstEmptyIndex = i;
                }
            }
            if (firstEmptyIndex != -1) {
                span[firstEmptyIndex].SetDataAndCount(itemData, count);
            }
            return firstEmptyIndex;
        }

        /// <summary>移除指定物品的数量，如果数量大于等于当前物品数量，则将该物品从背包中移除。</summary>
        /// <param name="itemData">指定的物品</param>
        /// <param name="count">要移除的数量</param>
        /// <returns>移除物品在背包中的索引，未找到或者添加数量为0或者物品数据为null，则返回-1</returns>
        public int RemoveItem(ItemData itemData, byte count) {
            if (itemData is null || count == 0) {
                return -1;
            }
            var span = new Span<InventoryItem>(_items);
            for (var i = 0; i < span.Length; ++i) {
                var item = span[i];
                if (item.Data == itemData) {
                    if (item.Count > count) {
                        item.Count -= count;
                    } else {
                        item.ResetDataAndCount();
                    }
                    return i;
                }
            }
            return -1;
        }

        public string ToJson() {
            var itemDatas = new InventoryItemData[_items.Length];
            for (int i = 0; i < _items.Length; i++) {
                var item = _items[i];
                itemDatas[i] = new InventoryItemData {
                    itemNameToken = item.Data?.nameToken,
                    itemCount = item.Count
                };
            }
            return JsonConvert.SerializeObject(itemDatas);
        }

        [Serializable]
        private struct InventoryItemData {
            public string itemNameToken;
            public byte itemCount;
        }
    }
}