using JoG.Character;
using JoG.DebugExtensions;
using System;
using Unity.Netcode;
using UnityEngine;
using VContainer.Unity;

namespace JoG.AISystem {

    public class EnemySpawner : NetworkBehaviour {
        public NetworkObject characterPrefab;
        public CharacterMaster characterMaster;
        [SerializeField] private GameObject[] _autoInjectObjects = Array.Empty<GameObject>();

        public void SpawnCharacter() {
            if (!HasAuthority) {
                this.LogError("No authority to spawn character.");
                return;
            }
            if (characterMaster == null) {
                this.LogError("Character master is null.");
                return;
            }
            if (!characterMaster.IsSpawned) {
                this.LogError("Character master is not spawned.");
                return;
            }
            transform.GetPositionAndRotation(out var position, out var rotation);
            var nob = NetworkManager.SpawnManager.InstantiateAndSpawn(
                characterPrefab,
                destroyWithScene: true,
                isPlayerObject: false,
                position: position,
                rotation: rotation);
            if (!nob.TrySetParent(characterMaster.NetworkObject, true)) {
                nob.Despawn();
            }
        }

        protected override void OnNetworkSessionSynchronized() {
            if (!HasAuthority) {
                return;
            }
            var masterNetworkObject = characterMaster.IsSpawned
                ? characterMaster.NetworkObject
                : characterMaster.GetComponent<NetworkObject>().InstantiateAndSpawn(NetworkManager, destroyWithScene: true);
            if (masterNetworkObject.TrySetParent(NetworkObject)) {
                SpawnCharacter();
            } else {
                this.LogError("Failed to set parent for character master.");
            }
        }

        protected void OnTransformChildrenChanged() {
            if (!IsSpawned) {
                return;
            }
            characterMaster = GetComponentInChildren<CharacterMaster>();
            if (characterMaster == null) {
                return;
            }
            var resolver = characterMaster.GetComponent<LifetimeScope>().Container;
            foreach (var obj in new ReadOnlySpan<GameObject>(_autoInjectObjects)) {
                resolver.InjectGameObject(obj);
            }
        }
    }
}