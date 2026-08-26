using EditorAttributes;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Video;

namespace Expriverse.Video {

    public class NetworkVideoPlayer : NetworkBehaviour {
        [Required, SerializeField] private VideoPlayer _videoPlayer;
        public VideoPlayer VideoPlayer => _videoPlayer;
        public bool IsPlaying => _videoPlayer.isPlaying;

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

        public void Stop() {
            StopRpc();
        }

        protected override void OnSynchronize<T>(ref BufferSerializer<T> serializer) {
            if (serializer.IsWriter) {
                var writer = serializer.GetFastBufferWriter();
                writer.WriteValueSafe(_videoPlayer.frame);
            } else {
                var reader = serializer.GetFastBufferReader();
                reader.ReadValueSafe(out long frame);
                if (frame > 0) {
                    _videoPlayer.frame = frame;
                    _videoPlayer.Play();
                }
            }
        }

        [Rpc(SendTo.Everyone)]
        private void PlayRpc() {
            _videoPlayer.Play();
        }

        [Rpc(SendTo.Everyone)]
        private void StopRpc() {
            _videoPlayer.Pause();
            _videoPlayer.frame = 1;
        }

        private void Start() {
            _videoPlayer.Prepare();
            _videoPlayer.prepareCompleted += PprepareCompleted;
        }

        private void PprepareCompleted(VideoPlayer source) {
            source.Pause();
            source.frame = 1;
        }
    }
}
