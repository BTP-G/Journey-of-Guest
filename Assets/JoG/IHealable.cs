using Unity.Netcode;

namespace JoG {

    public interface IHealable {

        void Handle(HealingMessage message);
    }
}