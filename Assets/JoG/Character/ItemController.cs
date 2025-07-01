using JoG.DebugExtensions;
using JoG.InventorySystem;
using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Runtime.InteropServices;
using JoG.InteractionSystem;

namespace JoG.Character {

    public class ItemController : MonoBehaviour, IItemPickUpController {
        private InventoryItem currentItem;
        private GameObject currentHandItem;
        public List<IItemHandler> Handlers { get; private set; }
        public IInventoryController InventoryController { get; set; }

        public void RemoveCurrentItemCount(byte count) {
            if (currentItem == null) {
                return;
            }
            if (currentItem.Count < count) {
                currentItem.Count = 0;
                this.LogWarning($"Not enough item count to remove: {currentItem.Name}, current count: {currentItem.Count}, requested count: {count}");
                return;
            }
            currentItem.Count -= count;
        }

        public void Use(InventoryItem item) {
            currentItem = item;

            // 销毁旧的手上物品
            if (currentHandItem != null) {
                if (currentHandItem.TryGetComponent<NetworkObject>(out var netObj) && netObj.IsSpawned) {
                    netObj.Despawn();
                } else {
                    Destroy(currentHandItem);
                }
            }

            GameObject spawned = null;
            if (item is not null && item.Prefab != null && item.Prefab.TryGetComponent(out NetworkObject prefab)) {
                var spawnedNetObj = NetworkManager.Singleton.SpawnManager.InstantiateAndSpawn(prefab, destroyWithScene: true);
                spawned = spawnedNetObj.gameObject;
            }

            currentHandItem = spawned;
            // 通知所有Handler
            foreach (var handler in Handlers.AsSpan()) {
                handler.Handle(spawned);
            }
        }

        void IInteractionMessageHandler.Handle(IInteractable interactableObject) {
            if(interactableObject is PickupItem pickupItem) {
                InventoryController.AddItem(pickupItem.itemData, pickupItem.count);
            }
        }

        protected virtual void Awake() {
            Handlers = new List<IItemHandler>();
            GetComponentsInChildren(true, Handlers);
        }

        protected virtual void OnDisable() {
            // 销毁手上物品
            Use(null);
        }
    }
}