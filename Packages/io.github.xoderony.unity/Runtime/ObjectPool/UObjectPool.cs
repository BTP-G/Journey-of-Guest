using UnityEngine;

namespace Xoderony.ObjectPool.Unity {

    public abstract class UObjectPool<T> : MonoBehaviour, IPool<T> where T : Object {
        public T Prefab { get; set; }

        public abstract T Rent();

        public abstract void Return(T uobj);

        public abstract void Clear();

        protected virtual void OnDestroy() {
            ObjectPoolManager<T>.RemovePool(Prefab);
        }
    }
}