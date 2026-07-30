using System.Collections.Generic;
using UnityEngine;

namespace Xoderony.ObjectPool.Unity {

    public abstract class ComponentPool<T> : UObjectPool<T> where T : Component {
        protected readonly Stack<T> stack = new(16);

        public override T Rent() {
            if (stack.TryPop(out var result)) {
                result.transform.SetParent(null);
            } else {
                result = Instantiate(Prefab);
            }
            return result;
        }

        public virtual T Rent(Transform parent, bool worldPositionStays = false) {
            if (stack.TryPop(out var result)) {
                result.transform.SetParent(parent, worldPositionStays);
            } else {
                result = Instantiate(Prefab, parent, worldPositionStays);
            }
            return result;
        }

        public virtual T Rent(in Vector3 position, in Quaternion rotation) {
            if (stack.TryPop(out var result)) {
                result.transform.SetParent(null);
                result.transform.SetPositionAndRotation(position, rotation);
            } else {
                result = Instantiate(Prefab, position, rotation);
            }
            return result;
        }

        public virtual T Rent(in Vector3 position, in Quaternion rotation, Transform parent) {
            if (stack.TryPop(out var result)) {
                result.transform.SetParent(parent, true);
                result.transform.SetPositionAndRotation(position, rotation);
            } else {
                result = Instantiate(Prefab, position, rotation, parent);
            }
            return result;
        }

        public override void Return(T component) {
            component.transform.SetParent(transform, false);
            component.gameObject.SetActive(false);
            stack.Push(component);
        }

        public override void Clear() {
            foreach (var t in stack) {
                Destroy(t.gameObject);
            }
            stack.Clear();
        }
    }
}