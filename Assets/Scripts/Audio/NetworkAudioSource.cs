using GuestUnion;
using System;
using Unity.Netcode;
using UnityEngine;

namespace JoG.Audio {

    public class NetworkAudioSource : NetworkBehaviour {
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip[] _audioClips = Array.Empty<AudioClip>();

        public void Play(byte audioIndex) {
            PlayRpc(audioIndex);
        }

        public void Play(string audioName) {
            PlayRpc((byte)_audioClips.FindIndex(ac => ac.name == audioName));
        }

        [Rpc(SendTo.Everyone)]
        private void PlayRpc(byte index) {
            _audioSource.PlayOneShot(_audioClips[index]);
        }
    }
}