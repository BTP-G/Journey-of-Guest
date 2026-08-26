using EditorAttributes;
using Expriverse.Item;
using Expriverse.UI;
using MessagePipe;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

namespace Expriverse.Inventory {

    public sealed class CharacterInventoryView : MonoBehaviour, IComponent, INetworkSpawnHandler, INetworkDespawnHandler {

        [Inject] internal CharacterInventory inventory;

        [Inject] internal CharacterItemDropController itemDropController;

        [Inject, Key(Constants.InputAction.Inventory)] internal InputAction inventoryToggle;

        [Inject] internal IPublisher<UIStateChangedMessage> publisher;

        [Inject] internal ScreenTooltip tooltipView;

        [SerializeField, Required] private CanvasGroup _group;

        [SerializeField, Required] private Slot _slotTemplate;

        private readonly Dictionary<ItemData, Slot> _itemToSlot = new();

        private readonly Stack<Slot> _slotPool = new();

        public bool IsOpen => _group.interactable;

        void INetworkSpawnHandler.OnSpawn(bool isOwner) {
            enabled = isOwner;
            if (!isOwner) {
                return;
            }

            foreach (var pair in inventory) {
                SetItemCount(pair.Key, pair.Value);
            }
            inventory.ItemCountChanged += SetItemCount;
            inventoryToggle.performed += OnInventoryToggle;
        }

        void INetworkDespawnHandler.OnDespawn(bool isOwner) {
            if (isOwner) {
                inventory.ItemCountChanged -= SetItemCount;
                inventoryToggle.performed -= OnInventoryToggle;
            }
            Close();
            ClearSlots();
            enabled = false;
        }

        private void Awake() {
            _group.alpha = 0;
            _group.interactable = false;
            _group.blocksRaycasts = false;
        }

        private void OnValidate() {
            enabled = false;
        }

        private void OnInventoryToggle(InputAction.CallbackContext callback) {
            if (IsOpen) {
                Close();
            } else {
                Open();
            }
        }

        private void Open() {
            if (IsOpen) {
                return;
            }
            _group.alpha = 1;
            _group.interactable = true;
            _group.blocksRaycasts = true;
            publisher.Publish(new("Inventory", UILayer.Overlay, true));
        }

        private void Close() {
            if (!IsOpen) {
                return;
            }
            _group.alpha = 0;
            _group.interactable = false;
            _group.blocksRaycasts = false;
            publisher.Publish(new("Inventory", UILayer.Overlay, false));
        }

        private void SetItemCount(ItemData itemData, int itemCount) {
            if (_itemToSlot.TryGetValue(itemData, out var slot)) {
                if (itemCount > 0) {
                    slot.ItemCount = itemCount;
                    return;
                }

                _itemToSlot.Remove(itemData);
                slot.DropRequested -= OnDropRequested;
                slot.gameObject.SetActive(false);
                _slotPool.Push(slot);
                return;
            }

            if (itemCount == 0) {
                return;
            }

            if (_slotPool.TryPop(out slot)) {
                slot.transform.SetAsLastSibling();
            } else {
                slot = Instantiate(_slotTemplate, _slotTemplate.transform.parent);
            }
            slot.tooltipView = tooltipView;
            slot.Initialize(itemData, itemCount);
            slot.DropRequested += OnDropRequested;
            slot.gameObject.SetActive(true);
            _itemToSlot[itemData] = slot;
        }

        private void OnDropRequested(ItemData itemData, int count) {
            itemDropController.Drop(itemData, count);
        }

        private void ClearSlots() {
            foreach (var slot in _itemToSlot.Values) {
                slot.DropRequested -= OnDropRequested;
                slot.gameObject.SetActive(false);
                _slotPool.Push(slot);
            }
            _itemToSlot.Clear();
        }
    }
}

