using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Expriverse.Networking {

    public class GenericPrefabInstanceHandler : INetworkPrefabInstanceHandler {
        public readonly NetworkManager manager;
        public readonly NetworkObject prefab;
        public readonly LifetimeScope parent;
        public readonly IObjectResolver container;
        private readonly Stack<NetworkObject> _pool = new();

        public GenericPrefabInstanceHandler(NetworkManager manager, NetworkObject prefab, LifetimeScope parent) {
            this.manager = manager;
            this.prefab = prefab;
            this.parent = parent;
            container = parent.Container;
        }

        public NetworkObject Instantiate(ulong ownerClientId, Vector3 position, Quaternion rotation) {
            if (_pool.TryPop(out var instance)) {
                instance.gameObject.SetActive(true);
                instance.transform.SetPositionAndRotation(position, rotation);
            } else if (prefab.TryGetComponent<LifetimeScope>(out var lifetimeScope)) {
                lifetimeScope.parentReference.Object = parent;
                instance = Object.Instantiate(prefab, position, rotation);
            } else if (prefab.TryGetComponent<Entity>(out var entityScope)) {
                entityScope.Parent = parent;
                instance = Object.Instantiate(prefab, position, rotation);
            } else {
                instance = container.Instantiate(prefab, position, rotation);
            }
            if (instance.TryGetComponent<Rigidbody>(out var rigidbody)) {
                rigidbody.position = position;
                rigidbody.rotation = rotation;
            }
            return instance;
        }

        public void Destroy(NetworkObject networkObject) {
            networkObject.gameObject.SetActive(false);
            _pool.Push(networkObject);
        }
    }
}
