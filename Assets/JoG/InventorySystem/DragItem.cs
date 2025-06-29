using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JoG.InventorySystem {

    [Serializable]
    public struct DragItem {
        public GameObject gameObject;
        public Transform transform;
        public Image iconImage;
        public TMP_Text countText;
    }
}