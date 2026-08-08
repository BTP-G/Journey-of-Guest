using System.Collections.Generic;
using UnityEngine;

namespace Xoderony.ObjectPool.Unity {

    public class GameObjectPool : UObjectPool<GameObject> {
        protected readonly Stack<GameObject> stack = new(16);

        public override GameObject Rent() {
            if (stack.TryPop(out var result)) {
                result.transform.SetParent(null);
            } else {
                result = Instantiate(Prefab);
            }
            return result;
        }

        public virtual GameObject Rent(Transform parent, bool worldPositionStays = false) {
            if (stack.TryPop(out var result)) {
                result.transform.SetParent(parent, worldPositionStays);
            } else {
                result = Instantiate(Prefab, parent, worldPositionStays);
            }
            return result;
        }

        public virtual GameObject Rent(in Vector3 position, in Quaternion rotation) {
            if (stack.TryPop(out var result)) {
                result.transform.SetParent(null);
                result.transform.SetPositionAndRotation(position, rotation);
            } else {
                result = Instantiate(Prefab, position, rotation);
            }
            return result;
        }

        public virtual GameObject Rent(in Vector3 position, in Quaternion rotation, Transform parent) {
            if (stack.TryPop(out var result)) {
                result.transform.SetParent(parent, true);
                result.transform.SetPositionAndRotation(position, rotation);
            } else {
                result = Instantiate(Prefab, position, rotation, parent);
            }
            return result;
        }

        public override void Return(GameObject go) {
            //go.transform.SetParent(transform, false);
            //go.SetActive(false);
            stack.Push(go);
        }

        public override void Clear() {
            foreach (var go in stack) {
                Destroy(go);
            }
            stack.Clear();
        }
    }
}