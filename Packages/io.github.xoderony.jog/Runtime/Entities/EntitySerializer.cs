using Unity.Netcode;

namespace JoG {

    public static class EntitySerializer {

        public static void WriteValueSafe(this FastBufferWriter writer, in Entity value) {
            var id = value != null ? value.Id : ulong.MaxValue;
            writer.WriteValueSafe(id);
        }

        public static void ReadValueSafe(this FastBufferReader reader, out Entity value) {
            reader.ReadValueSafe(out ulong id);
            Entity.IdToEntity.TryGetValue(id, out value);
        }
    }
}
