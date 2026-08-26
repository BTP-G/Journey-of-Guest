using System.Collections.Generic;
using UnityEngine;
using VContainer;
using Xoderony;
using Xoderony.Movement;

namespace Expriverse.Character {

    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CharacterMotor))]
    public class CharacterBody : MonoBehaviour, IComponent, INetworkSpawnHandler, INetworkDespawnHandler, INetworkAuthorityChangedHandler {
        public readonly List<Collider> colliders = new();
        public Animator Animator { get; private set; }
        public Rigidbody Rigidbody { get; private set; }
        public CharacterMotor Motor { get; private set; }

        [Inject]
        internal void Inject(
            IDelegateSubscriber<CharacterLifeStartHandler> lifeStarted,
            IDelegateSubscriber<CharacterLifeStopHandler> lifeStopped) {

            lifeStarted.Subscribe(OnLifeStart);
            lifeStopped.Subscribe(OnLifeStop);
        }

        void INetworkSpawnHandler.OnSpawn(bool isOwner) {
            transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        }

        void INetworkDespawnHandler.OnDespawn(bool isOwner) {
            transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        }

        void INetworkAuthorityChangedHandler.OnAuthorityChanged(bool hasAuthority) {
            Motor.enabled = hasAuthority;
        }

        private void Awake() {
            Animator = GetComponent<Animator>();
            Rigidbody = GetComponent<Rigidbody>();
            Motor = GetComponent<CharacterMotor>();
            GetComponentsInChildren(true, colliders);
            foreach (var collider in colliders) {
                if (collider.TryGetComponent<HurtBox>(out _)) {
                    collider.enabled = false;
                }
            }
        }

        private void OnLifeStart(CharacterEntity entity) {
            foreach (var collider in colliders) {
                if (collider.TryGetComponent<HurtBox>(out _)) {
                    collider.enabled = true;
                }
            }
        }

        private void OnLifeStop(CharacterEntity entity) {
            foreach (var collider in colliders) {
                if (collider.TryGetComponent<HurtBox>(out _)) {
                    collider.enabled = false;
                }
            }
        }
    }
}
