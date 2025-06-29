using JoG.InventorySystem;

namespace JoG.Messages {

    public struct InventoryChangedMessage {
        public int index;
        public Inventory inventory;
        public InventoryItem item;
    }
}