using ANU.IngameDebug.Console;
using EditorAttributes;
using Xoderony.Localization;
using Xoderony.Logging;
using JoG.Character;
using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace JoG {

    [DebugCommandPrefix("player")]
    public class PlayerSpawner : CharacterSpawner {

        [ReadOnly]
        public NetworkObject spawnedPlayerCharacter;

        public GameObject cardTemplate;

        [Inject]
        internal CharacterDataDictionary nameToData;

        [Inject]
        internal NetworkManager networkManager;

        private Transform _spawnPoint;

        [DebugCommand(Description = "Spawn a default body at a random spawn point")]
        public void SpawnPlayerCharacter(string name) {
            try {
                var card = nameToData[name];
                SpawnPlayer(card.networkPrefab);
            } catch (Exception ex) {
                this.LogException(ex);
            }
        }

        public void SpawnPlayer(NetworkObject prefab) {
            if ((spawnedPlayerCharacter != null) && spawnedPlayerCharacter.IsSpawned) {
                spawnedPlayerCharacter.Despawn();
            }
            _spawnPoint.GetPositionAndRotation(out var position, out var rotation);
            var entity = SpawnBody(prefab, position, rotation, true);
            spawnedPlayerCharacter = entity.NetworkObject;
        }

        public override void OnBodySpawn(CharacterEntity entity) {
            base.OnBodySpawn(entity);
            if (entity.HasAuthority) {
                entity.Health.Current = entity.Health.Max;
            }
        }

        protected void Awake() {
            _spawnPoint = GameObject.FindGameObjectWithTag("PlayerSpawnPoint")
                                    .transform;
            foreach (var cardData in nameToData.Datas) {
                var prefab = cardData.networkPrefab;
                var spawnCard = Instantiate(cardTemplate, cardTemplate.transform.parent);
                spawnCard.GetComponent<Image>()
                         .sprite = cardData.iconSprite;
                spawnCard.GetComponentInChildren<TMP_Text>()
                         .text = Localizer.GetString(cardData.nameKey);
                spawnCard.GetComponent<Button>()
                         .onClick
                         .AddListener(() => SpawnPlayer(prefab));
                spawnCard.SetActive(true);
            }
        }

    }

}
