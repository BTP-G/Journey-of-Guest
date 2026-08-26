using EditorAttributes;
using Unity.Netcode;
using UnityEngine;

namespace Expriverse.Audio {

    public class NetworkAudioSource : NetworkBehaviour {
        [Required, SerializeField] private AudioSource _audioSource;

        public AudioSource AudioSource => _audioSource;
        public bool IsPlaying => _audioSource.isPlaying;

        public void TogglePlay() {
            if (IsPlaying) {
                StopRpc();
            } else {
                PlayRpc();
            }
        }

        public void Play() {
            PlayRpc();
        }

        public void PlayOneShot() {
            PlayOneShotRpc();
        }

        public void Stop() {
            StopRpc();
        }

        protected override void OnSynchronize<T>(ref BufferSerializer<T> serializer) {
            if (serializer.IsWriter) {
                var writer = serializer.GetFastBufferWriter();
                writer.WriteValueSafe(_audioSource.time);
            } else {
                var reader = serializer.GetFastBufferReader();
                reader.ReadValueSafe(out float time);
                if (time > 0) {
                    _audioSource.time = time;
                    _audioSource.Play();
                }
            }
        }

        [Rpc(SendTo.Everyone)]
        private void PlayRpc() {
            _audioSource.Play();
        }

        [Rpc(SendTo.Everyone)]
        private void PlayOneShotRpc() {
            _audioSource.PlayOneShot(_audioSource.clip);
        }

        [Rpc(SendTo.Everyone)]
        private void StopRpc() {
            _audioSource.Stop();
        }
    }
}
