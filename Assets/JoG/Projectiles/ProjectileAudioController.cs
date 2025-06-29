using Unity.Netcode;
using UnityEngine;

namespace JoG.Projectiles {

    public class ProjectileAudioController : NetworkBehaviour {
        public AudioSource audioSource;

        public override void OnNetworkSpawn() {
            audioSource.Play();
        }
    }
}