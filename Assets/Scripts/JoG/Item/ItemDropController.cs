using JoG.Health;
using Xoderony.YooAsset;
using JoG.Character;
using JoG.Networking;
using MessagePipe;
using System;
using Unity.Netcode;
using UnityEngine;
using VContainer;
using URandom = UnityEngine.Random;

namespace JoG.Item {

    [DisallowMultipleComponent]
    public class ItemDropController : NetworkBehaviour, IMessageHandler<DeathMessage> {
        public YooAssetReference<ItemDropTable> dropTableReference;
        public float dropChance = 0.5f;
        [Inject] internal ISubscriber<DeathMessage> deathMessageSubscriber;
        [Inject] internal NetworkObjectFactory networkObjectFactory;
        private IDisposable subscription;

        void IMessageHandler<DeathMessage>.Handle(DeathMessage message) {
            if (HasAuthority
                && message.entity is CharacterEntity characterEntity
                && characterEntity.GetComponent<Faction>().Id == Constants.Factions.Enemy) {
                if (dropChance >= 1 || URandom.value < dropChance) {
                    var dropItem = dropTableReference.AssetObject.Get();
                    var item = networkObjectFactory.Instantiate(
                         dropItem,
                         position: characterEntity.Model.Center,
                         rotation: Quaternion.identity);
                    item.GetComponent<ItemPickupBehaviour>().Amount = 1;
                    item.Spawn(true);
                    item.GetComponent<Rigidbody>().linearVelocity = -Physics.gravity;
                }
            }
        }

        public override void OnDestroy() {
            base.OnDestroy();
            dropTableReference.Unload();
        }

        private void Awake() {
            dropTableReference.Load();
        }

        private void OnEnable() {
            subscription = deathMessageSubscriber.Subscribe(this);
        }

        private void OnDisable() {
            subscription?.Dispose();
        }
    }
}
