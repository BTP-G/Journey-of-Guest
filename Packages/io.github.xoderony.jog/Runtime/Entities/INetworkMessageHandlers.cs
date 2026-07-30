using Unity.Netcode;

namespace JoG {

    public interface INetworkSpawnHandler {

        void OnSpawn(bool isOwner);
    }

    public interface INetworkDespawnHandler {

        void OnDespawn(bool isOwner);
    }

    public interface INetworkOwnershipChangeHandler {

        void OnLostOwnership(bool isPreviousOwner);

        void OnGainedOwnership(bool isCurrentOwner);
    }

    public interface INetworkSynchronizeHandler {

        void OnSynchronize<T>(ref BufferSerializer<T> serializer) where T : IReaderWriter;
    }
}
