using System.Collections.Generic;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace Xoderony.ObjectPool.Unity {

    public static class ObjectPoolManager<TElement> where TElement : UObject {
        private static Dictionary<int, UObjectPool<TElement>> hashToPool = new();

        public static TPool GetPool<TPool>(TElement prefab) where TPool : UObjectPool<TElement> {
            if (!hashToPool.TryGetValue(prefab.GetHashCode(), out var pool)) {
                hashToPool[prefab.GetHashCode()] = pool = new GameObject(typeof(TElement).Name + "Pool").AddComponent<TPool>();
                pool.Prefab = prefab;
            }
            return pool as TPool;
        }

        public static UObjectPool<TElement> RemovePool(TElement prefab) {
            hashToPool.Remove(prefab.GetHashCode(), out var pool);
            return pool;
        }
    }
}