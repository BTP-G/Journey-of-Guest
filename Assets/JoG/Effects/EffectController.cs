using JoG.DebugExtensions;
using Unity.Netcode;
using UnityEngine;

namespace JoG.Effects {

    public class EffectController : NetworkBehaviour {
        [field: SerializeField] public ParticleSystem ParticleSystem { get; private set; }

        protected void OnParticleSystemStopped() {
            if (HasAuthority) {
                NetworkObject.Despawn();
            }
        }
    }
}