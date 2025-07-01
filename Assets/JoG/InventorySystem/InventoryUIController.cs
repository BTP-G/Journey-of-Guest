using EditorAttributes;
using GuestUnion;
using JoG.Messages;
using MessagePipe;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using VContainer;

namespace JoG.InventorySystem {

    public class InventoryUIController : MonoBehaviour {

        [Header("依赖组件")]
        [Required] public InventoryController inventoryController;

        public GameObject tablePanel;
        public GameObject hotBarPanel;
        public DragItem dragItem;
        public Color selectedColor = Color.yellow;
        public Color normalColor = Color.white;
        [Inject] private IPublisher<CharacterInputLockMessage> _publisher;
        private List<Slot> slots = new();
        private Slot selectedSlot;

        public void RefreshAllSlots() {
            var span = slots.AsSpan();
            for (var i = 0; i < span.Length; ++i) {
                span[i].UpdateView(inventoryController.inventory[i]);
            }
        }

        public void RefreshSlot(int index) {
            if (index >= 0 && index < slots.Count)
                slots[index].UpdateView(inventoryController.inventory[index]);
        }

        public void HighlightAt(int selectedIndex) {
            if (selectedSlot != null) {
                selectedSlot.iconImage.color = normalColor;
            }
            if (slots.TryGetAt(selectedIndex, out selectedSlot)) {
                selectedSlot.iconImage.color = selectedColor;
            }
        }

        public void ShowDragItem(Sprite iconSprite, string countText) {
            dragItem.iconImage.sprite = iconSprite;
            dragItem.countText.text = countText;
            dragItem.gameObject.SetActive(true);
        }

        public void SetDragItemPosition(Vector2 position) {
            dragItem.transform.localPosition = position;
        }

        public void HideDragItem() {
            dragItem.gameObject.SetActive(false);
        }

        public void ExchangeItem(int from, int to) {
            inventoryController.ExchangeItem(from, to);
        }

        private void Awake() {
            GetComponentsInChildren(true, slots);
            for (var i = 0; i < slots.Count; i++) {
                var slot = slots[i];
                slot.controller = this;
                slot.index = i;
            }
        }

        private void Start() {
            RefreshAllSlots();
            HighlightAt(inventoryController.selectedIndex);
        }

        private void Update() {
            // Tab键切换背包面板
            if (UnityEngine.InputSystem.Keyboard.current.tabKey.wasPressedThisFrame) {
                tablePanel.SetActive(!tablePanel.activeSelf);
                _publisher.Publish(new CharacterInputLockMessage(tablePanel.activeSelf));
            }
        }
    }
}