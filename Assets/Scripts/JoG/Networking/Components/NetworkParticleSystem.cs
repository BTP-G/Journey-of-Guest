using Unity.Netcode;
using UnityEngine;

namespace JoG.Networking.Components {

    [RequireComponent(typeof(ParticleSystem))]
    public class NetworkParticleSystem : NetworkBehaviour {
        private ParticleSystem _particleSystem;

        public ParticleSystem ParticleSystem => _particleSystem;
        public bool IsPlaying => _particleSystem.isPlaying;

        public void TogglePlay() {
            if (IsPlaying) {
                StopRpc();
            } else {
                PlayRpc();
            }
        }

        public void Play() {
            if (IsPlaying) return;
            PlayRpc();
        }

        public void Stop() {
            if (IsPlaying) {
                StopRpc();
            }
        }

        protected override void OnSynchronize<T>(ref BufferSerializer<T> serializer) {
            if (serializer.IsWriter) {
                var writer = serializer.GetFastBufferWriter();
                writer.WriteValueSafe(_particleSystem.time);
            } else {
                var reader = serializer.GetFastBufferReader();
                reader.ReadValueSafe(out float time);
                if (time > 0) {
                    _particleSystem.time = time;
                    _particleSystem.Play(true);
                }
            }
        }

        private void Awake() {
            _particleSystem = GetComponent<ParticleSystem>();
        }

        [Rpc(SendTo.Everyone)]
        private void PlayRpc() => _particleSystem.Play(true);

        [Rpc(SendTo.Everyone)]
        private void StopRpc() => _particleSystem.Stop(true);
    }
}
