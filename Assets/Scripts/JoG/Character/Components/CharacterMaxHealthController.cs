using JoG.Health;
using System;
using VContainer;

namespace JoG.Character.Components {

    [Serializable]
    public sealed class CharacterMaxHealthController : IComponent, INetworkSpawnHandler, INetworkDespawnHandler {

        [Inject] internal HealthComponent health;

        [Inject, Key(Constants.Stats.MaxHealth)] internal Stat maxHealth;

        void INetworkSpawnHandler.OnSpawn(bool isOwner) {
            maxHealth.ValueChanged += ApplyMaxHealth;
            ApplyMaxHealth();
        }

        void INetworkDespawnHandler.OnDespawn(bool isOwner) {
            maxHealth.ValueChanged -= ApplyMaxHealth;
        }

        private void ApplyMaxHealth() {
            health.Max = maxHealth.Value;
        }

    }

}
