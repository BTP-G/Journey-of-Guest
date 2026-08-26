using Expriverse.Item;
using Unity.Netcode;
using VContainer;

namespace Expriverse.Inventory {

    public sealed class CharacterInventoryNetwork : NetworkBehaviour, IComponent {

        [Inject] internal CharacterInventory inventory;

        public override void OnNetworkDespawn() {
            base.OnNetworkDespawn();
            inventory.ClearSilently();
        }

        [Rpc(SendTo.Owner)]
        public void AddItemRpc(ItemData item, int count = 1) {
            inventory.AddItem(item, count);
        }

        [Rpc(SendTo.Owner)]
        public void RemoveItemRpc(ItemData item, int count = 1) {
            inventory.RemoveItem(item, count);
        }
    }
}

