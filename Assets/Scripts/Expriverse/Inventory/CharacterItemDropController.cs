using Expriverse.Character;
using Expriverse.Item;
using Expriverse.Networking;
using System;
using UnityEngine;
using VContainer;

namespace Expriverse.Inventory {

    [Serializable]
    public sealed class CharacterItemDropController : IComponent {

        [Inject] internal CharacterEntity entity;

        [Inject] internal CharacterInventory inventory;

        [Inject] internal NetworkObjectFactory networkObjectFactory;

        public bool Drop(ItemData itemData, int count) {
            if (count <= 0) {
                throw new ArgumentOutOfRangeException(nameof(count), count, "Item count must be greater than zero.");
            }
            if (!inventory.HasEnoughItems(itemData, count)) {
                return false;
            }

            var position = entity.Model.Center;
            var velocity = entity.Rigidbody.GetPointVelocity(position);
            velocity += entity.Rigidbody.rotation * new Vector3(0, 5, 1);

            var item = networkObjectFactory.Instantiate(
                itemData.pickupPrefab,
                position: position,
                rotation: Quaternion.identity
            );
            item.GetComponent<ItemPickupBehaviour>().Amount = count;
            item.Spawn(true);
            item.GetComponent<Rigidbody>().linearVelocity = velocity;
            return inventory.RemoveItem(itemData, count);
        }
    }
}

