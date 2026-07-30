using System;
using VContainer;

namespace JoG.Health {

    [Serializable]
    public sealed class HealthComponentChangeResolver : IComponent, IHealthChangeResolver {

        [Inject]
        internal Entity entity;

        [Inject]
        internal HealthComponent health;

        public HealthChangeReport Resolve(Entity source, ref HealthChangeMessage message) {
            var oldHealth = health.Current;
            health.Current += message.Value;
            return new HealthChangeReport {
                source = source,
                target = entity,
                flags = message.Flags,
                color = message.Color,
                value = message.Value,
                deltaValue = health.Current - oldHealth,
                position = message.Position
            };
        }
    }
}
