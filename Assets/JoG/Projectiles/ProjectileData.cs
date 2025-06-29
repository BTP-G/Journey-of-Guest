using Unity.Netcode;

namespace JoG.Projectiles {

    public class ProjectileData : NetworkBehaviour {
        public NetworkObjectReference ownerReference;

        public override void OnNetworkDespawn() {
            ownerReference = default;
        }
    }
}