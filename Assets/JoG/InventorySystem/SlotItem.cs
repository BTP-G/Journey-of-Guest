using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JoG.InventorySystem {

    public class SlotItem : MonoBehaviour {
        public Image iconImage;

        public TMP_Text countText;

        private void Awake() {
            iconImage = GetComponent<Image>();
            countText = GetComponentInChildren<TMP_Text>(true);
        }
    }
}