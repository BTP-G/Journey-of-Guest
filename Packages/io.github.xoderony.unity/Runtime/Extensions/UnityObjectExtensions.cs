using System.Runtime.CompilerServices;
using UnityEngine;

namespace Xoderony.Extensions {

    public static class UnityObjectExtensions {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Destroy(this Object obj, float t = 0f) {
            Object.Destroy(obj, t);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Clone<T>(this T obj) where T : Object {
            return Object.Instantiate(obj);
        }

    }

}