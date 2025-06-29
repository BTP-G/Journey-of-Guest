using EditorAttributes;
using JoG.Character;
using JoG.DebugExtensions;
using JoG.Messages;
using MessagePipe;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using VContainer;

namespace JoG.InventorySystem {

    public class InventoryController : MonoBehaviour, IInventoryController, IMessageHandler<CharacterBodyChangedMessage> {
        public Inventory inventory = new(60);
        public InputActionReference numberInput;
        public ItemData itemToUse;
        public int selectedIndex = 0;
        private CharacterBody _body;
        private IDisposable _disposable;
        private ItemController _itemController;

        // UI控制器引用
        public InventoryUIController uiController;

        [Button]
        public void AddItem(int count) {
            var index = inventory.AddItem(itemToUse, (byte)count);
            uiController?.RefreshSlot(index);
        }

        [Button]
        public void ExchangeItem(int fromIndex, int toIndex) {
            inventory.ExchangeItemSafe(fromIndex, toIndex);
            uiController?.RefreshSlot(fromIndex);
            uiController?.RefreshSlot(toIndex);
        }

        [Inject]
        public void Construct(IBufferedSubscriber<CharacterBodyChangedMessage> subscriber) {
            _disposable = subscriber.Subscribe(this);
        }

        void IMessageHandler<CharacterBodyChangedMessage>.Handle(CharacterBodyChangedMessage message) {
            _body = message.next;
            if (_body is null) {
                _itemController = null;
                return;
            }
            _itemController = _body.GetComponent<ItemController>();
            var item = inventory.GetItemSafe(selectedIndex);
            if (item.Count > 0) {
                _itemController.Use(item);
            }
        }

        public void AddItem(ItemData item, byte count) => inventory.AddItem(item, count);

        public void RemoveItem(int index, byte count) => inventory.RemoveItem(index);

        public void RemoveItem(ItemData item, byte count) => inventory.RemoveItem(item, count);

        public void SelectItem(int index) {
            selectedIndex = index;
            var item = inventory.GetItemSafe(selectedIndex);
            if (item.Count > 0) {
                _itemController.Use(item);
            }
            uiController?.HighlightAt(selectedIndex);
        }

        private void Awake() {
            // 数据初始化
            PlayerPrefs.DeleteKey("player_inventory");
            var inventoryStr = PlayerPrefs.GetString("player_inventory", string.Empty);
            inventory = Inventory.FromJson(inventoryStr) ?? new Inventory(60);
        }

        private void Start() {
            numberInput.action.performed += OnNumInput;
            numberInput.action.Enable();
        }

        private void OnNumInput(InputAction.CallbackContext context) {
            int idx = -1;
            switch ((context.control as KeyControl).keyCode) {
                case Key.Digit1:
                case Key.Numpad1: idx = 0; break;
                case Key.Digit2:
                case Key.Numpad2: idx = 1; break;
                case Key.Digit3:
                case Key.Numpad3: idx = 2; break;
                case Key.Digit4:
                case Key.Numpad4: idx = 3; break;
                case Key.Digit5:
                case Key.Numpad5: idx = 4; break;
                case Key.Digit6:
                case Key.Numpad6: idx = 5; break;
                case Key.Digit7:
                case Key.Numpad7: idx = 6; break;
                case Key.Digit8:
                case Key.Numpad8: idx = 7; break;
                case Key.Digit9:
                case Key.Numpad9: idx = 8; break;
                case Key.Digit0:
                case Key.Numpad0: idx = 9; break;
            }
            if (idx >= 0) {
                SelectItem(idx);
            }
            this.Log((context.control as KeyControl).keyCode);
        }

        private void OnDestroy() {
            PlayerPrefs.SetString("player_inventory", inventory.ToJson());
            PlayerPrefs.Save();
            numberInput.action.Disable();
            _disposable?.Dispose();
        }
    }
}