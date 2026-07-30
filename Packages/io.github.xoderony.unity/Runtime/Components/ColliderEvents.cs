using System;
using UnityEngine;

namespace Xoderony.Unity {

    [RequireComponent(typeof(Collider))]
    [DisallowMultipleComponent]
    [AddComponentMenu("Event/Collider Events")]
    public class ColliderEvents : MonoBehaviour {
        private Collider _collider;
        public Collider Collider => _collider;

        public event Action<Collision> CollisionEnter;

        public event Action<Collision> CollisionStay;

        public event Action<Collision> CollisionExit;

        public event Action<Collider> TriggerEnter;

        public event Action<Collider> TriggerStay;

        public event Action<Collider> TriggerExit;

        private void Awake() {
            _collider = GetComponent<Collider>();
        }

        private void OnCollisionEnter(Collision collision) {
            CollisionEnter?.Invoke(collision);
        }

        private void OnCollisionStay(Collision collision) {
            CollisionStay?.Invoke(collision);
        }

        private void OnCollisionExit(Collision collision) {
            CollisionExit?.Invoke(collision);
        }

        private void OnTriggerEnter(Collider other) {
            TriggerEnter?.Invoke(other);
        }

        private void OnTriggerStay(Collider other) {
            TriggerStay?.Invoke(other);
        }

        private void OnTriggerExit(Collider other) {
            TriggerExit?.Invoke(other);
        }
    }
}
