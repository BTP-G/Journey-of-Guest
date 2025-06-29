namespace JoG.Projectiles {

    public interface IProjectileHitMessageHandler {

        void Handle(in ProjectileHitMessage message);
    }
}