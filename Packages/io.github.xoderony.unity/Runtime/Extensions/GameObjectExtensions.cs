using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Pool;
using UObject = UnityEngine.Object;

namespace Xoderony.Extensions {

    public static class GameObjectExtensions {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LayerMask GetCollisionLayerMask(this GameObject gameObject) {
            var mask = 0;
            var colliderLayer = gameObject.layer;
            for (var i = 0; i < 32; ++i) {
                if (Physics.GetIgnoreLayerCollision(colliderLayer, i)) {
                    continue;
                }
                mask |= 1 << i;
            }
            return mask;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveComponent<T>(this GameObject gameObject) where T : Component {
            if (gameObject.TryGetComponent<T>(out var component)) {
                UObject.Destroy(component);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component {
            return gameObject.TryGetComponent<T>(out var component) ? component : gameObject.AddComponent<T>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveComponents<T>(this GameObject gameObject) where T : Component {
            using var _ = ListPool<T>.Get(out var buffer);
            gameObject.GetComponents(buffer);
            foreach (var component in buffer) {
                UObject.Destroy(component);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] GetComponentsInChildren<T>(this Component component, string tag, bool includeInactive = true) where T : Component {
            using var _ = ListPool<T>.Get(out var buffer);
            component.GetComponentsInChildren(includeInactive, buffer);
            buffer.RemoveAll(c => !c.CompareTag(tag));
            return buffer.ToArray();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] GetComponentsInChildren<T>(this GameObject go, string tag, bool includeInactive = true) where T : Component {
            using var _ = ListPool<T>.Get(out var buffer);
            go.GetComponentsInChildren(includeInactive, buffer);
            buffer.RemoveAll(c => !c.CompareTag(tag));
            return buffer.ToArray();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GetComponentsInChildren<T>(this Component component, List<T> result, string tag, bool includeInactive = true) where T : Component {
            component.GetComponentsInChildren(includeInactive, result);
            result.RemoveAll(c => !c.CompareTag(tag));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GetComponentsInChildren<T>(this GameObject go, List<T> result, string tag, bool includeInactive = true) where T : Component {
            go.GetComponentsInChildren(includeInactive, result);
            result.RemoveAll(c => !c.CompareTag(tag));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetComponentInChildren<T>(this GameObject go, string tag, bool includeInactive = true) where T : Component {
            var result = go.GetComponentInChildren<T>(includeInactive);
            return ((result is not null) && result.CompareTag(tag)) ? result : null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetComponentInChildren<T>(this Component component, string tag, bool includeInactive = true) where T : Component {
            var result = component.GetComponentInChildren<T>(includeInactive);
            return ((result is not null) && result.CompareTag(tag)) ? result : null;
        }

    }

}
