using Unity.Netcode;

namespace Expriverse {

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

    public interface INetworkAuthorityChangedHandler {

        void OnAuthorityChanged(bool hasAuthority);
    }

    public interface INetworkSynchronizeHandler {

        void OnSynchronize<T>(ref BufferSerializer<T> serializer) where T : IReaderWriter;
    }
}
