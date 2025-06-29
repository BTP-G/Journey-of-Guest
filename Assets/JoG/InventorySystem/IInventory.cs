using System.Collections.Generic;

namespace JoG.InventorySystem {

    public interface IInventory<TItem> where TItem : IItem {
        public ICollection<TItem> Items { get; }
    }
}