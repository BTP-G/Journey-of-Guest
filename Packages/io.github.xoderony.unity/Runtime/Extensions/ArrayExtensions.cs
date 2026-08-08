using UnityEngine;

namespace Xoderony.Extensions {

    public static class ArrayRandomExtensions {

        public static T GetRandomElement<T>(this T[] array) {
            var randomIndex = Random.Range(0, array.Length);
            return array[randomIndex];
        }
    }
}
