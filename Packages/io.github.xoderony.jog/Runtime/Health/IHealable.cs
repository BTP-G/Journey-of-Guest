namespace JoG.Health {

    public interface IHealable {

        Entity Entity { get; }

        bool CanTakeHeal(Entity attacker);

        void TakeHeal(ref HealthChangeMessage message, Entity attacker);
    }
}
