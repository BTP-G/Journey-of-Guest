using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Xoderony.Unity {

    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public class Billboarder : MonoBehaviour {

        private readonly List<Transform> _billboards = new();

        private Transform _transform;

        public void Register(Transform billboard) {
            Assert.IsNotNull(billboard);
            _billboards.Add(billboard);
        }

        public void Unregister(Transform billboard) {
            Assert.IsNotNull(billboard);
            _billboards.Remove(billboard);
        }

        private void Awake() {
            _transform = transform;
        }

        private void LateUpdate() {
            foreach (var billboard in _billboards) {
                billboard.rotation = _transform.rotation;
            }
        }
    }
}
