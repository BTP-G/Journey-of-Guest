using Unity.Netcode;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace JoG.Networking {

    internal class NetworkPlayerPrefabHandler : INetworkPrefabInstanceHandler {
        public readonly NetworkManager manager;
        public readonly NetworkObject prefab;
        public readonly IObjectResolver container;

        public NetworkPlayerPrefabHandler(NetworkManager manager, NetworkObject prefab, IObjectResolver container) {
            this.manager = manager;
            this.prefab = prefab;
            this.container = container;
        }

        public NetworkObject Instantiate(ulong ownerClientId, Vector3 position, Quaternion rotation) {
            return container.Instantiate(prefab, position, rotation);
        }

        public void Destroy(NetworkObject networkObject) {
            Object.Destroy(networkObject.gameObject);
        }
    }
}
