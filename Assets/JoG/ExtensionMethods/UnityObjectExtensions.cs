using UnityEngine;

namespace JoG.UnityObjectExtensions {

    public static class UnityObjectExtensions {

        public static void Destroy(this Object obj, float t = 0f) => Object.Destroy(obj, t);

        public static T Clone<T>(this T obj) where T : Object => (T)Object.Instantiate(obj);
    }
}