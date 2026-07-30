using Unity.Netcode;
using VContainer;

namespace JoG.Gameplay {

    public class DifficultyManager : NetworkBehaviour {
        public DifficultyConfig config;

        [Inject] internal IPlayerRegistry playerRegistry;
        [Inject] internal NetworkManager networkManager;

        public float CurrentHealthMultiplier {
            get {
                var value = config.hpMultiplierCurve.Evaluate(networkManager.ServerTime.TimeAsFloat * 0.1666666f);
                value *= config.playerCountToMultiplierCurve.Evaluate(playerRegistry.PlayerCount);
                return value;
            }
        }

        public float CurrentAttackMultiplier {
            get {
                var value = config.atkMultiplierCurve.Evaluate(networkManager.ServerTime.TimeAsFloat * 0.1666666f);
                value *= config.playerCountToMultiplierCurve.Evaluate(playerRegistry.PlayerCount);
                return value;
            }
        }

        public float CurrentDropChance => config.timeToDropChanceCurve.Evaluate(networkManager.ServerTime.TimeAsFloat * 0.1666666f);
    }
}
