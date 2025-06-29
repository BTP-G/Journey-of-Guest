using System.Collections.Generic;
using UnityEngine;

namespace JoG {

    public class SpawnPoint : MonoBehaviour {
        private static readonly List<Transform> spawnPoints = new();
        public static IReadOnlyList<Transform> SpawnPoints => spawnPoints;

        private void Awake() {
            spawnPoints.Add(transform);
        }

        private void OnDestroy() {
            spawnPoints.Remove(transform);
        }
    }
}