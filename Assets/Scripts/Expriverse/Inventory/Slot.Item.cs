using EditorAttributes;
using Expriverse.Item;
using UnityEngine;
using UnityEngine.Assertions;

namespace Expriverse.Inventory {

    public partial class Slot {
        [ReadOnly, SerializeField] private ItemData _itemData;
        [ReadOnly, SerializeField] private int _itemCount;
        public ItemData ItemData {
            get => _itemData;
            private set {
                Assert.IsNotNull(value);
                _itemData = value;
                iconImage.sprite = _itemData.iconSprite;
            }
        }

        public int ItemCount {
            get => _itemCount;
            internal set {
                if (_itemCount == value) {
                    return;
                }

                _itemCount = value < 0 ? 0 : value;
                countText.text = _itemCount.ToString();
            }
        }

        public bool IsEmpty => _itemCount == 0;

        public void Initialize(ItemData item, int itemCount) {
            ItemData = item;
            ItemCount = itemCount;
        }
    }
}

