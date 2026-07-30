using System;

using UnityEngine;

namespace Xoderony.Unity {

    [RequireComponent(typeof(ParticleSystem))]
    [DisallowMultipleComponent]
    [AddComponentMenu("Event/Particle System Events")]
    public class ParticleSystemEvents : MonoBehaviour {
        private ParticleSystem _particleSystem;
        public ParticleSystem ParticleSystem => _particleSystem;

        public event Action<ParticleSystemEvents, GameObject> ParticleCollision;

        public event Action<ParticleSystemEvents> ParticleTrigger;

        public event Action<ParticleSystemEvents> ParticleSystemStopped;

        private void Awake() {
            _particleSystem = GetComponent<ParticleSystem>();
        }

        private void OnParticleCollision(GameObject other) {
            ParticleCollision?.Invoke(this,other);
        }

        private void OnParticleTrigger() {
            ParticleTrigger?.Invoke(this);
        }

        private void OnParticleSystemStopped() {
            ParticleSystemStopped?.Invoke(this);
        }
    }
}
