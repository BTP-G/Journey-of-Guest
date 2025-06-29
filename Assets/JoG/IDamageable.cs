using Unity.Netcode;

namespace JoG {

    public interface IDamageable {

        void Handle(DamageMessage message);
    }
}