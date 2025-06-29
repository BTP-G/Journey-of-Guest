using Unity.Netcode;

namespace JoG {

    [System.Serializable]
    public struct HealingMessage : INetworkSerializable {
        public static readonly int Size = sizeof(uint) + sizeof(float) + sizeof(ulong);
        public uint value;
        public float cofficient;
        public ulong flags;
        public NetworkObjectReference healer;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
            if (serializer.PreCheck(Size)) {
                serializer.SerializeValuePreChecked(ref value);
                serializer.SerializeValuePreChecked(ref cofficient);
                serializer.SerializeValuePreChecked(ref flags);
            }
            serializer.SerializeNetworkSerializable(ref healer);
        }
    }
}