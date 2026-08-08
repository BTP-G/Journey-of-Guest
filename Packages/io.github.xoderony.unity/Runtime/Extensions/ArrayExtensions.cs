using UnityEngine;

namespace Xoderony.Extensions.Unity {

    public static class ArrayExtensions {

        public static T GetRandomElement<T>(this T[] array) {
            var randomIndex = Random.Range(0, array.Length);
            return array[randomIndex];
        }
    }
}