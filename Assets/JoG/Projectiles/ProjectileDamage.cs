using Unity.Netcode;
using UnityEngine;

namespace JoG.Projectiles {

    [RequireComponent(typeof(ProjectileData))]
    public class ProjectileDamage : MonoBehaviour, IProjectileHitMessageHandler {
        public uint damageValue;
        public float force;
        public NetworkObject explosionEffectPrefab;
        private ProjectileData _data;

        public void Handle(in ProjectileHitMessage message) {
            if (_data.HasAuthority && message.collider.TryGetComponent<IDamageable>(out var damageable)) {
                damageable.Handle(new DamageMessage {
                    value = damageValue,
                    cofficient = 1,
                    flags = DamgeFlag.magic | DamgeFlag.sharp,
                    position = message.position,
                    impulse = force * transform.forward,
                    attacker = _data.ownerReference,
                });
            }
            explosionEffectPrefab.InstantiateAndSpawn(
                _data.NetworkManager,
                position: message.position,
                rotation: Quaternion.LookRotation(message.normal));
        }

        protected void Awake() {
            _data = GetComponent<ProjectileData>();
            
        }
    }
}