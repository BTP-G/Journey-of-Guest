using Unity.Netcode;

namespace Expriverse.Item {

    public static class ItemDataSerializer {

        public static void WriteValueSafe(this FastBufferWriter writer, in ItemData itemData) {
            writer.WriteValueSafe(itemData.name);
        }

        public static void ReadValueSafe(this FastBufferReader reader, out ItemData itemData) {
            reader.ReadValueSafe(out string dataName);
            itemData = ItemDataDictionary.Shared[dataName];
        }
    }
}
