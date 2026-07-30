namespace JoG.Health {

    public interface IDamageable {

        Entity Entity { get; }

        bool CanTakeDamage(Entity attacker);

        void TakeDamage(ref HealthChangeMessage message, Entity attacker);
    }
}
