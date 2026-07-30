using UnityEngine;

namespace Xoderony.Extensions {

    public static class ColliderExtensions {

        public static LayerMask GetCollisionLayerMask(this Collider collider) {
            var mask = 0;
            var colliderLayer = collider.gameObject.layer;
            for (var i = 0; i < 32; ++i) {
                if (Physics.GetIgnoreLayerCollision(colliderLayer, i)) {
                    continue;
                }
                mask |= (1 << i);
            }
            return mask;
        }
    }
}