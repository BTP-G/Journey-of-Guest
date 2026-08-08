using JoG.Networking;
using Unity.Netcode;
using UnityEngine;
using VContainer;
using Xoderony.YooAsset;

namespace JoG.Item {

    public class ItemSpawner : NetworkBehaviour {
        public YooAssetReference<GameObject> prefab;
        [Inject] internal NetworkObjectFactory networkObjectFactory;
        private NetworkObject networkPrefab;

        public override void OnDestroy() {
            base.OnDestroy();
            prefab.Unload();
        }

        protected override void OnInSceneObjectsSpawned() {
            base.OnInSceneObjectsSpawned();
            if (!HasAuthority) {
                return;
            }

            transform.GetPositionAndRotation(out var position, out var rotation);
            var item = networkObjectFactory.Instantiate(networkPrefab, position: position, rotation: rotation);
            item.GetComponent<ItemPickupBehaviour>().Amount = 1;
            item.Spawn(true);
        }

        private void Awake() {
            prefab.Load();
            networkPrefab = prefab.AssetObject.GetComponent<NetworkObject>();
        }
    }
}
