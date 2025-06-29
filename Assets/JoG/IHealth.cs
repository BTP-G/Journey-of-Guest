namespace JoG {

    public interface IHealth {
        uint HP { get; }
        uint MaxHP { get; }
        float PercentHp { get; }
        bool IsAlive { get; }

        void Handle(in DamageMessage message);

        void Handle(in HealingMessage message);
    }
}