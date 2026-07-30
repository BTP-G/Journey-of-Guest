using UnityEngine;

namespace JoG.Audio {

    [System.Serializable]
    public class AudioPlayer {
        public AudioSource audioSourcePrefab;
        public AudioClip audioClip;

        public void Play(in Vector3 position, in Quaternion rotation) {
            audioSourcePrefab.clip = audioClip;
            Object.Instantiate(audioSourcePrefab, position, rotation);
        }
    }
}
