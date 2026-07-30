using Xoderony;
using JoG.Health;
using MessagePipe;
using System;
using VContainer;

namespace JoG.Character.Components {

    [Serializable]
    public sealed class CharacterLifeController : IComponent, INetworkSpawnHandler, INetworkDespawnHandler {

        [Inject] internal IPublisher<DeathMessage> deathPublisher;

        [Inject] internal CharacterEntity entity;

        [Inject] internal HealthComponent health;

        [Inject] internal IDelegateSubscriber<HealthChangedHandler> healthChanged;

        [Inject] internal IDelegateDispatcher<CharacterLifeStartHandler> lifeStartedHandlers;

        [Inject] internal IDelegateDispatcher<CharacterLifeStopHandler> lifeStoppedHandlers;

        void INetworkSpawnHandler.OnSpawn(bool isOwner) {
            if (isOwner) {
                health.Current = health.Max;
            }

            healthChanged.Subscribe(OnHealthChanged);
            if (health.IsAlive) {
                NotifyLifeStart();
            } else {
                NotifyLifeStop();
            }
        }

        void INetworkDespawnHandler.OnDespawn(bool isOwner) {
            healthChanged.Unsubscribe(OnHealthChanged);
        }

        private void NotifyDeath() {
            deathPublisher.Publish(new DeathMessage {
                entity = entity,
            });
            NotifyLifeStop();
            entity.Spawner.OnBodyLifeStop(entity);
        }

        private void NotifyLifeStart() {
            lifeStartedHandlers.Handlers?.Invoke(entity);
            entity.Spawner.OnBodyLifeStart(entity);
        }

        private void NotifyLifeStop() {
            lifeStoppedHandlers.Handlers?.Invoke(entity);
        }

        private void OnHealthChanged(int prev, int next) {
            if (prev <= 0 && next > 0) {
                NotifyLifeStart();
            } else if (prev > 0 && next <= 0) {
                NotifyDeath();
            }
        }

    }

}
