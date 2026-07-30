using Xoderony.ObjectPool.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace JoG.Character.States {

    [RequireComponent(typeof(Collider))]
    public class HitBox : MonoBehaviour {

        public TriggerEvent onHit = new();

        public void Activate(float duration) {
            Enable();
            Invoke(nameof(Disable), duration);
        }

        public void Enable() {
            gameObject.SetActive(true);
        }

        public void Disable() {
            gameObject.SetActive(false);
        }

        private void Awake() {
            var collider = GetComponent<Collider>();
            using (ListPool<Collider>.Rent(out var result)) {
                transform.parent.GetComponentsInChildren(true, result);
                foreach (var collider2 in result) {
                    Physics.IgnoreCollision(collider, collider2, true);
                }
            }
        }

        private void OnTriggerEnter(Collider other) {
            onHit.Invoke(other);
        }

        [Serializable]
        public class TriggerEvent : UnityEvent<Collider> { }

    }

}
