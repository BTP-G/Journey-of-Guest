using JoG.Health;
using System;
using VContainer;
using Xoderony;

namespace JoG.Character.Components {

    [Serializable]
    public sealed class CharacterHurtBoxLifeController : IComponent, INetworkSpawnHandler, INetworkDespawnHandler {

        [Inject] internal Entity entity;

        [Inject] internal HealthComponent health;

        [Inject] internal IDelegateSubscriber<CharacterLifeStartHandler> lifeStarted;

        [Inject] internal IDelegateSubscriber<CharacterLifeStopHandler> lifeStopped;

        void INetworkSpawnHandler.OnSpawn(bool isOwner) {
            lifeStarted.Subscribe(OnLifeStart);
            lifeStopped.Subscribe(OnLifeStop);
            SetHurtBoxesEnabled(health.IsAlive);
        }

        void INetworkDespawnHandler.OnDespawn(bool isOwner) {
            lifeStarted.Unsubscribe(OnLifeStart);
            lifeStopped.Unsubscribe(OnLifeStop);
            SetHurtBoxesEnabled(false);
        }

        private void OnLifeStart(CharacterEntity entity) {
            SetHurtBoxesEnabled(true);
        }

        private void OnLifeStop(CharacterEntity entity) {
            SetHurtBoxesEnabled(false);
        }

        private void SetHurtBoxesEnabled(bool enabled) {
            foreach (var collider in entity.Colliders) {
                if (collider.TryGetComponent<HurtBox>(out _)) {
                    collider.enabled = enabled;
                }
            }
        }
    }
}
