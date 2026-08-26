namespace Expriverse.Health {

    public interface IHittable {

        Entity Entity { get; }

        void TakeHit(in HitMessage message, Entity source);
    }
}
