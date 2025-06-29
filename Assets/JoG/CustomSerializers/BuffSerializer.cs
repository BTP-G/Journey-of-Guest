using JoG.BuffSystem;
using Unity.Netcode;

namespace JoG.CustomSerializers {

    public static class BuffSerializer {

        public static void WriteValueSafe(this FastBufferWriter writer, in IBuff buff) {
            BytePacker.WriteValueBitPacked(writer, buff.Index);
            buff.Serialize(writer);
        }

        public static void ReadValueSafe(this FastBufferReader reader, out IBuff buff) {
            ByteUnpacker.ReadValueBitPacked(reader, out ushort buffIndex);
            buff = BuffPool.Rent(buffIndex);
            buff.Deserialize(reader);
        }
    }
}