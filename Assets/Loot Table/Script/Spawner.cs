using UnityEngine;
using UnityEngine.Events;

namespace TinyScript {
    public class Spawner : MonoBehaviour {
        [Header("Press [E, R, T, Y]")]
        public LootDrop Loot;
        public int AdditionalDropChange = 2;
        public float MinSpawnRadius = 0.5f;
        public float MaxSpawnRadius = 1;
        [Header("Press [F]")]
        public UnityEvent OnDropCube;
        [Header("Press [G]")]
        public UnityEvent OnDropCubeSphere;

        private void Update() {
            if (Input.GetKeyDown(KeyCode.E)) {
                Loot.SpawnDrop(transform, AdditionalDropChange, MinSpawnRadius, MaxSpawnRadius);
            }
            if (Input.GetKeyDown(KeyCode.R)) {
                Loot.SpawnDrop(transform, AdditionalDropChange, MaxSpawnRadius);
            }
            if (Input.GetKeyDown(KeyCode.T)) {
                Loot.SpawnDrop(transform, AdditionalDropChange, MinSpawnRadius, MaxSpawnRadius, false);
            }
            if (Input.GetKeyDown(KeyCode.Y)) {
                Loot.SpawnDrop(transform, AdditionalDropChange, MaxSpawnRadius, false);
            }

            if (Input.GetKeyDown(KeyCode.F)) {
                OnDropCube.Invoke();
            }
            if (Input.GetKeyDown(KeyCode.G)) {
                OnDropCubeSphere.Invoke();
            }
        }

        private void OnDrawGizmosSelected() {
            Loot.GizmoDrawSpawnRange(transform, MinSpawnRadius, MaxSpawnRadius);
        }
    }
}
