using Xoderony.YooAsset;
using Unity.Netcode;
using UnityEngine;

namespace JoG.Networking {

    [DisallowMultipleComponent]
    public class SessionOwnerObjectSpawner : NetworkBehaviour {
        public YooAssetReference<GameObject> prefab;
        private NetworkObject networkPrefab;

        public override void OnDestroy() {
            base.OnDestroy();
            prefab.Unload();
        }

        protected override void OnNetworkPostSpawn() {
            if (!IsSessionOwner) return;
            base.OnNetworkPostSpawn();
            transform.GetPositionAndRotation(out var position, out var rotation);
            NetworkManager.SpawnManager.InstantiateAndSpawn(networkPrefab,
                destroyWithScene: true,
                position: position,
                rotation: rotation
            );
        }

        private void Awake() {
            prefab.Load();
            networkPrefab = prefab.AssetObject.GetComponent<NetworkObject>();
        }
    }
}
