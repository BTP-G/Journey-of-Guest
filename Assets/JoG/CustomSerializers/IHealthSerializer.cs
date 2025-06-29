using Unity.Netcode;

namespace JoG.CustomSerializers {

    public static class IHealthSerializer {

        public static void WriteValueSafe(this FastBufferWriter writer, in IHealth health) {
            writer.WriteNetworkSerializable(new NetworkBehaviourReference(health as NetworkBehaviour));
        }

        public static void ReadValueSafe(this FastBufferReader reader, out IHealth health) {
            reader.ReadNetworkSerializable(out NetworkBehaviourReference networkBehaviourRef);
            networkBehaviourRef.TryGet(out var networkBehaviour);
            health = networkBehaviour as IHealth;
        }
    }
}