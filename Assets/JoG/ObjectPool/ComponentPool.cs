using UnityEngine;

namespace JoG.ObjectPool {

    public class ComponentPool<T> : UObjectPool<T> where T : Component {

        public new T Rent() {
            var result = base.Rent();
            result.gameObject.SetActive(true);
            return result;
        }

        public T Rent(Transform parent, bool worldPositionStays = false) {
            if (pool.TryPop(out var result)) {
                result.transform.SetParent(parent, worldPositionStays);
            } else {
                result = Instantiate(Prefab, parent, worldPositionStays);
            }
            result.gameObject.SetActive(true);
            return result;
        }

        public T Rent(in Vector3 position, in Quaternion rotation) {
            if (pool.TryPop(out var result)) {
                result.transform.SetPositionAndRotation(position, rotation);
            } else {
                result = Instantiate(Prefab, position, rotation);
            }
            result.gameObject.SetActive(true);
            return result;
        }

        public T Rent(in Vector3 position, in Quaternion rotation, Transform parent) {
            if (pool.TryPop(out var result)) {
                result.transform.SetParent(parent, true);
                result.transform.SetPositionAndRotation(position, rotation);
            } else {
                result = Instantiate(Prefab, position, rotation, parent);
            }
            result.gameObject.SetActive(true);
            return result;
        }

        public new void Return(T component) {
            component.transform.SetParent(null,false);
            component.gameObject.SetActive(false);
            base.Return(component);
        }
    }
}