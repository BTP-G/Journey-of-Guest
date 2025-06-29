using JoG.Messages;
using MessagePipe;
using Unity.Netcode;
using UnityEngine;

namespace JoG.InventorySystem {

    [System.Serializable]
    public class InventoryItem {
        public readonly Inventory inventory;

        private ItemData _data;
        private byte _count;
        private int _index;
        public int Index { get => _index; internal set => _index = value; }
        public string Name => _data?.nameToken;

        public string Description {
            get {
                return _data?.descriptionToken;
            }
        }

        public Sprite Icon => _data?.icon;

        public GameObject Prefab => _data?.prefab;

        public byte Count {
            get => _count;
            set {
                _count = value;
                if (value == 0) {
                    _data = null;
                }
                inventory.PublishItemChanged(_index);
            }
        }

        public ItemData Data {
            get => _data;
            set {
                _data = value;
                if (value == null) {
                    _count = 0;
                }
                inventory.PublishItemChanged(_index);
            }
        }

        public InventoryItem(Inventory inventory, int index) {
            this.inventory = inventory;
            _index = index;
        }

        public void SetDataAndCount(ItemData itemData, byte itemCount) {
            _data = itemData;
            _count = itemCount;
            inventory.PublishItemChanged(_index);
        }

        public void ResetDataAndCount() {
            _data = null;
            _count = 0;
            inventory.PublishItemChanged(_index);
        }
    }
}