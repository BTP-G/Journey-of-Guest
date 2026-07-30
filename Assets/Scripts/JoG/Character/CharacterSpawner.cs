using JoG.Networking;
using Unity.Netcode;
using UnityEngine;
using VContainer;

namespace JoG.Character {

    public class CharacterSpawner : NetworkBehaviour {

        [Inject] internal NetworkObjectFactory networkObjectFactory;

        public CharacterEntity Body { get; private set; }

        public CharacterEntity SpawnBody(NetworkObject networkPrefab, in Vector3 position, in Quaternion rotation, bool isPlayer = false) {
            var bodyObj = networkObjectFactory.Instantiate(
                networkPrefab,
                position: position,
                rotation: rotation
            );
            var entity = bodyObj.GetComponent<CharacterEntity>();
            entity.Spawner = this;
            if (isPlayer) {
                bodyObj.SpawnAsPlayerObject(NetworkManager.LocalClientId, true);
            } else {
                bodyObj.Spawn(true);
            }
            return entity;
        }

        public virtual void OnBodySpawn(CharacterEntity entity) {
            Body = entity;
        }

        public virtual void OnBodyDespawn(CharacterEntity entity) { }

        public virtual void OnBodyLifeStart(CharacterEntity entity) { }

        public virtual void OnBodyLifeStop(CharacterEntity entity) { }

    }

}
