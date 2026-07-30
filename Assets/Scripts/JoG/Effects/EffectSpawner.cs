using EditorAttributes;
using Xoderony.Extensions;
using Xoderony.ObjectPool.Unity;
using Xoderony.Unity;
using UnityEngine;

namespace JoG.VisualEffects {

    public class EffectSpawner : MonoBehaviour {

        [Required]
        public GameObject prefab;

        private GameObjectPool _pool;

        public void Spawn() {
            Spawn(transform);
        }

        public void Spawn(Transform point) {
            point.GetPositionAndRotation(out var position, out var rotation);
            Spawn(position, rotation);
        }

        public void Spawn(Vector3 position, Quaternion rotation) {
            var events = _pool.Rent(position, rotation)
                              .GetOrAddComponent<ParticleSystemEvents>();
            events.ParticleSystemStopped += OnEffectStopped;
            events.gameObject
                  .SetActive(true);
        }

        private void OnEffectStopped(ParticleSystemEvents events) {
            if (this == null) {
                events.gameObject
                      .Destroy();
                return;
            }
            events.ParticleSystemStopped -= OnEffectStopped;
            events.gameObject
                  .SetActive(false);
            _pool.Return(events.gameObject);
        }

        private void Awake() {
            _pool = ObjectPoolManager<GameObject>.GetPool<GameObjectPool>(prefab);
        }

    }

}
