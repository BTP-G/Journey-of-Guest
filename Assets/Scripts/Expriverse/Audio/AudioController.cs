using UnityEngine;

namespace Expriverse.Audio {

    [RequireComponent(typeof(AudioSource))]
    public class AudioController : MonoBehaviour {
        protected AudioSource _audioSource;

        protected void Awake() {
            _audioSource = GetComponent<AudioSource>();
            _audioSource.playOnAwake = true;
        }

        protected void OnEnable() {
            Destroy(_audioSource, _audioSource.clip.length);
        }
    }
}
