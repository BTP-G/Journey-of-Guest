using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace JoG.InventorySystem {

    [CreateAssetMenu(fileName = "ItemCollector", menuName = "Inventory/Item Collector")]
    public class ItemCollector : ScriptableObject {
        public ItemData[] itemDefs = Array.Empty<ItemData>();
        private Dictionary<string, ItemData> _nameToItemDatas;
        public static ItemCollector Instance { get; private set; }

        public bool TryGetItemDef(string itemName, out ItemData itemData) => _nameToItemDatas.TryGetValue(itemName, out itemData);

        public void Awake() {
            Instance = this;
            _nameToItemDatas = itemDefs.ToDictionary(static item => item.nameToken, static item => item);
        }
    }
}