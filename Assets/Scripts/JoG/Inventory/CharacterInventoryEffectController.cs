using JoG.Character;
using JoG.Item;
using System;
using System.Collections.Generic;
using VContainer;

namespace JoG.Inventory {

    [Serializable]
    public sealed class CharacterInventoryEffectController : IComponent, INetworkSpawnHandler, INetworkDespawnHandler {

        [Inject] internal CharacterEffects effects;

        [Inject] internal CharacterInventory inventory;

        private readonly Dictionary<ItemData, int> _itemToAppliedCount = new();

        void INetworkSpawnHandler.OnSpawn(bool isOwner) {
            if (!isOwner) {
                return;
            }

            foreach (var pair in inventory) {
                ApplyCountDelta(pair.Key, pair.Value);
            }
            inventory.ItemCountChanged += OnItemCountChanged;
        }

        void INetworkDespawnHandler.OnDespawn(bool isOwner) {
            if (isOwner) {
                inventory.ItemCountChanged -= OnItemCountChanged;
            }
            _itemToAppliedCount.Clear();
        }

        private void OnItemCountChanged(ItemData item, int count) {
            ApplyCountDelta(item, count);
        }

        private void ApplyCountDelta(ItemData item, int count) {
            _itemToAppliedCount.TryGetValue(item, out var appliedCount);
            var countDelta = count - appliedCount;
            if (countDelta > 0) {
                effects.AddEffectRpc(item.Id, countDelta);
            } else if (countDelta < 0) {
                effects.RemoveEffectRpc(item.Id, -countDelta);
            }

            if (count == 0) {
                _itemToAppliedCount.Remove(item);
            } else {
                _itemToAppliedCount[item] = count;
            }
        }
    }
}
