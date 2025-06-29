namespace JoG.InventorySystem {

    public interface IInventoryController {

        void AddItem(ItemData item, byte count);

        void ExchangeItem(int fromIndex, int toIndex);

        void RemoveItem(int index, byte count);

        void RemoveItem(ItemData item, byte count);
    }
}