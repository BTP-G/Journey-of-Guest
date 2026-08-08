using EditorAttributes;
using Unity.Netcode;
using UnityEngine;
using Xoderony.Extensions;
using Xoderony.ObjectPool.Unity;
using Xoderony.Unity;

namespace JoG.Networking.Components {

    public class NetworkEffectSpawner : NetworkBehaviour {
        [Required] public GameObject prefab;
        private GameObjectPool _pool;

        [Rpc(SendTo.Everyone)]
        public void SpawnRpc(Vector3 position, Quaternion rotation) {
            var events = _pool.Rent(position, rotation).GetOrAddComponent<ParticleSystemEvents>();
            events.ParticleSystemStopped += OnEffectStopped;
            events.gameObject.SetActive(true);
        }

        private void OnEffectStopped(ParticleSystemEvents events) {
            events.ParticleSystemStopped -= OnEffectStopped;
            events.gameObject.SetActive(false);
            _pool.Return(events.gameObject);
        }

        private void Awake() {
            _pool = ObjectPoolManager<GameObject>.GetPool<GameObjectPool>(prefab);
        }
    }
}
