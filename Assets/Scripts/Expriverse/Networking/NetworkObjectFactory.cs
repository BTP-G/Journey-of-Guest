using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Expriverse.Networking {

    public sealed class NetworkObjectFactory {

        private readonly NetworkManager _networkManager;

        private readonly Dictionary<NetworkObject, INetworkPrefabInstanceHandler> _handlers = new();

        public NetworkObjectFactory(NetworkManager networkManager) {
            _networkManager = networkManager;
        }

        public bool AddHandler(NetworkObject networkPrefab, INetworkPrefabInstanceHandler handler) {
            if (networkPrefab == null) {
                throw new ArgumentNullException(nameof(networkPrefab));
            }
            if (handler == null) {
                throw new ArgumentNullException(nameof(handler));
            }
            if (_handlers.ContainsKey(networkPrefab)) {
                return false;
            }
            if (!_networkManager.PrefabHandler.AddHandler(networkPrefab, handler)) {
                return false;
            }
            _handlers.Add(networkPrefab, handler);
            return true;
        }

        public bool RemoveHandler(NetworkObject networkPrefab, INetworkPrefabInstanceHandler handler) {
            if ((networkPrefab == null) || (handler == null)) {
                return false;
            }
            if (!_handlers.TryGetValue(networkPrefab, out var registeredHandler) || !ReferenceEquals(registeredHandler, handler)) {
                return false;
            }
            _handlers.Remove(networkPrefab);
            return _networkManager.PrefabHandler.RemoveHandler(networkPrefab);
        }

        public NetworkObject Instantiate(NetworkObject networkPrefab, ulong ownerClientId = NetworkManager.ServerClientId, bool forceOverride = false, Vector3 position = default, Quaternion rotation = default) {
            if (networkPrefab == null) {
                throw new ArgumentNullException(nameof(networkPrefab));
            }
            if (!_networkManager.NetworkConfig.Prefabs.Contains(networkPrefab.gameObject)) {
                throw new ArgumentException($"{networkPrefab.name} is not registered as a network prefab.", nameof(networkPrefab));
            }

            ownerClientId = _networkManager.DistributedAuthorityMode ? _networkManager.LocalClientId : ownerClientId;

            if (_handlers.TryGetValue(networkPrefab, out var handler)) {
                return handler.Instantiate(
                    ownerClientId,
                    position,
                    rotation
                );
            }

            var prefab = networkPrefab.gameObject;
            if (forceOverride || _networkManager.IsClient || _networkManager.DistributedAuthorityMode) {
                prefab = _networkManager.GetNetworkPrefabOverride(prefab);
            }

            var instance = UnityEngine.Object
                                      .Instantiate(
                                          prefab,
                                          position,
                                          rotation
                                      );
            if (instance.TryGetComponent<NetworkObject>(out var networkObject)) {
                return networkObject;
            }

            UnityEngine.Object.Destroy(instance);
            throw new InvalidOperationException($"The instantiated network prefab {prefab.name} has no {nameof(NetworkObject)} component.");
        }
    }
}
