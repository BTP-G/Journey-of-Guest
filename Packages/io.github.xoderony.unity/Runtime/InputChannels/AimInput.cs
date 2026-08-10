using UnityEngine;

namespace Xoderony.InputChannels {

    public readonly struct AimInput {
        public readonly Vector3 position;
        public readonly Transform target;

        public AimInput(Vector3 position, Transform target) {
            this.position = position;
            this.target = target;
        }
    }
}
