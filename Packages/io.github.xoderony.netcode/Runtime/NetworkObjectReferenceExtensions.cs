using System.Runtime.CompilerServices;
using Unity.Netcode;

namespace Xoderony.Extensions {

    public static class NetworkObjectReferenceExtensions {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetComponent<T>(this NetworkObjectReference reference) where T : class {
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(reference.NetworkObjectId, out var networkObject)) {
                return networkObject.GetComponent<T>();
            }
            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetComponent<T>(this NetworkObjectReference reference, out T component) where T : class {
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(reference.NetworkObjectId, out var networkObject)) {
                return networkObject.TryGetComponent(out component);
            }
            component = null;
            return false;
        }
    }
}