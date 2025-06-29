using System.Collections.Generic;
using UnityEngine;

namespace JoG.ObjectPool {

    public class UObjectPool<T> : ScriptableObject where T : Object {
        protected readonly Stack<T> pool = new(7);
        [field: SerializeField] public T Prefab { get; private set; }

        public T Rent() => pool.TryPop(out var result) ? result : Instantiate(Prefab);

        public void Return(T uobj) => pool.Push(uobj);

        protected void OnDisable() {
            pool.Clear();
        }
    }
}