using UnityEngine;

namespace JoG.ObjectPool {

    [CreateAssetMenu(fileName = nameof(GameObjectPool), menuName = nameof(ObjectPool) + "/" + nameof(GameObjectPool))]
    public class GameObjectPool : UObjectPool<GameObject> {

        public new GameObject Rent() {
            var result = base.Rent();
            result.SetActive(true);
            return result;
        }

        public GameObject Rent(Transform parent, bool worldPositionStays = false) {
            if (pool.TryPop(out var result)) {
                result.transform.SetParent(parent, worldPositionStays);
            } else {
                result = Instantiate(Prefab, parent, worldPositionStays);
            }
            result.SetActive(true);
            return result;
        }

        public GameObject Rent(in Vector3 position, in Quaternion rotation) {
            if (pool.TryPop(out var result)) {
                result.transform.SetPositionAndRotation(position, rotation);
            } else {
                result = Instantiate(Prefab, position, rotation);
            }
            result.SetActive(true);
            return result;
        }

        public GameObject Rent(in Vector3 position, in Quaternion rotation, Transform parent) {
            if (pool.TryPop(out var result)) {
                result.transform.SetPositionAndRotation(position, rotation);
            } else {
                result = Instantiate(Prefab, position, rotation, parent);
            }
            result.SetActive(true);
            return result;
        }

        public new void Return(GameObject go) {
            go.SetActive(false);
            base.Return(go);
        }
    }
}