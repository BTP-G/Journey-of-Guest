using JoG.Networking.Components;
using UnityEngine;

namespace JoG.Projectiles {

    /// <summary>定点放置射弹：Initialize 强类型初始化，生命到期销毁时按能力组件响应。</summary>
    public sealed class PlacedProjectile : ProjectileEntity {
        [SerializeField] private NetworkEffectSpawner spawnEffect;

        private ProjectileExplosion _explosion;
        private Entity _attacker;
        private float _damageValue;

        public void Initialize(Entity owner, Entity attacker, float damageValue) {
            SetOwner(owner);
            _attacker = attacker;
            _damageValue = damageValue;
        }

        protected new void Awake() {
            base.Awake();
            TryGetComponent(out _explosion);
        }

        public override void OnNetworkSpawn() {
            base.OnNetworkSpawn();
            if (IsOwner) {
                spawnEffect?.SpawnRpc(transform.position, transform.rotation);
            }
        }

        public override void OnNetworkDespawn() {
            if (IsOwner) {
                _explosion?.Detonate(_attacker, transform.position, _damageValue);
            }
            base.OnNetworkDespawn();
        }
    }
}
