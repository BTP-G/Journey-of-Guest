namespace JoG.InventorySystem {

    public interface IInventoryController {

        int AddItem(ItemData item, byte count);

        void ExchangeItem(int fromIndex, int toIndex);

        void RemoveItem(int index, byte count);

        int RemoveItem(ItemData item, byte count);
    }
}