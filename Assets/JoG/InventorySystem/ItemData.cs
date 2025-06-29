using System;
using Unity.Netcode;
using UnityEngine;

namespace JoG.InventorySystem {

    [CreateAssetMenu(fileName = "New Item Data", menuName = "Inventory/ItemData")]
    public class ItemData : ScriptableObject {
        public string nameToken;
        public string descriptionToken;
        public Sprite icon;
        public GameObject prefab;
    }
}