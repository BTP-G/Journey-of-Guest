using System.Collections.Generic;
using UnityEngine;

namespace Xoderony.ObjectPool.Unity {

    public class PooledObject : MonoBehaviour {
        protected List<GameObject> pool;

        public GameObject Rent() {
            pool ??= new List<GameObject>(16);
            GameObject result;
            if (pool.Count > 0) {
                var lastIndex = pool.Count - 1;
                result = pool[lastIndex];
                pool.RemoveAt(lastIndex);
            } else {
                var pooledObject = Instantiate(this);
                pooledObject.pool = pool;
                result = pooledObject.gameObject;
            }
            result.SetActive(true);
            return result;
        }

        public GameObject Rent(Transform parent, bool worldPositionStays = false) {
            pool ??= new List<GameObject>(16);
            GameObject result;
            if (pool.Count > 0) {
                var lastIndex = pool.Count - 1;
                result = pool[lastIndex];
                pool.RemoveAt(lastIndex);
                result.transform.SetParent(parent, worldPositionStays);
            } else {
                var pooledObject = Instantiate(this, parent, worldPositionStays);
                pooledObject.pool = pool;
                result = pooledObject.gameObject;
            }
            result.SetActive(true);
            return result;
        }

        public GameObject Rent(in Vector3 position, in Quaternion rotation) {
            pool ??= new List<GameObject>(16);
            GameObject result;
            if (pool.Count > 0) {
                var lastIndex = pool.Count - 1;
                result = pool[lastIndex];
                pool.RemoveAt(lastIndex);
                result.transform.SetPositionAndRotation(position, rotation);
            } else {
                var pooledObject = Instantiate(this, position, rotation);
                pooledObject.pool = pool;
                result = pooledObject.gameObject;
            }
            result.SetActive(true);
            return result;
        }

        public GameObject Rent(in Vector3 position, in Quaternion rotation, Transform parent) {
            pool ??= new List<GameObject>(16);
            GameObject result;
            if (pool.Count > 0) {
                var lastIndex = pool.Count - 1;
                result = pool[lastIndex];
                pool.RemoveAt(lastIndex);
                result.transform.SetPositionAndRotation(position, rotation);
                result.transform.SetParent(parent);
            } else {
                var pooledObject = Instantiate(this, position, rotation, parent);
                pooledObject.pool = pool;
                result = pooledObject.gameObject;
            }
            result.SetActive(true);
            return result;
        }

        protected void OnDisable() {
            pool?.Add(gameObject);
        }

        protected void OnDestroy() {
            pool?.Remove(gameObject);
        }
    }
}