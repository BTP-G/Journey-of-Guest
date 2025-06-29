using System.Collections.Generic;
using UnityEngine;

namespace JoG.GameObjectExtensions {

    public static class GameObjectExtensions {

        public static IReadOnlyList<T> GetComponentsNonAlloc<T>(this GameObject gameObject) where T : class {
            var components = ListCache<T>.cache;
            gameObject.GetComponents(components);
            return components;
        }

        public static IReadOnlyList<T> GetComponentsInChildrenNonAlloc<T>(this GameObject gameObject, bool includeInactive = false) where T : class {
            var components = ListCache<T>.cache;
            gameObject.GetComponentsInChildren(includeInactive, components);
            return components;
        }

        public static IReadOnlyList<T> GetComponentsInParentNonAlloc<T>(this GameObject gameObject, bool includeInactive = false) where T : class {
            var components = ListCache<T>.cache;
            gameObject.GetComponentsInParent(includeInactive, components);
            return components;
        }

        private static class ListCache<T> {
            public static readonly List<T> cache = new(1);
        }
    }
}