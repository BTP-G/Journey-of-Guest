using JoG.Health;
using JoG.Networking.Components;
using UnityEngine;
using UnityEngine.Assertions;

namespace JoG.Projectiles {

    /// <summary>直线飞行射弹：Initialize 强类型初始化，命中时按能力组件组合响应。</summary>
    public sealed class LinearProjectile : ProjectileEntity {
        [Min(0)] public float speed = 20f;

        [SerializeField] private NetworkEffectSpawner impactEffect;

        private ProjectileDamage _damage;
        private ProjectileExplosion _explosion;
        private ProjectileDot _dot;
        private ProjectilePenetration _penetration;
        private ProjectileDespawn _despawn;
        private Rigidbody _rigidbody;
        private Collider _collider;
        private Entity _attacker;
        private float _damageValue;
        private Collider[] _ignoreColliders;

        public void Initialize(Entity owner, Entity attacker, float damageValue, in Vector3 inheritedVelocity, Collider[] ignoreColliders) {
            SetOwner(owner);
            _attacker = attacker;
            _damageValue = damageValue;
            _ignoreColliders = ignoreColliders;
            _rigidbody.linearVelocity = inheritedVelocity + (transform.rotation * new Vector3(0, 0, speed));
            foreach (var ignoreCollider in ignoreColliders) {
                Physics.IgnoreCollision(_collider, ignoreCollider, true);
            }
        }

        public override void OnNetworkSpawn() {
            base.OnNetworkSpawn();
            _collider.enabled = IsOwner;
        }

        public override void OnNetworkDespawn() {
            base.OnNetworkDespawn();
            _collider.enabled = false;
            if (!IsOwner) {
                return;
            }
            foreach (var ignoreCollider in _ignoreColliders) {
                Physics.IgnoreCollision(_collider, ignoreCollider, false);
            }
        }

        protected new void Awake() {
            base.Awake();
            _rigidbody = gameObject.GetComponent<Rigidbody>();
            _collider = gameObject.GetComponent<Collider>();
            TryGetComponent(out _damage);
            TryGetComponent(out _explosion);
            TryGetComponent(out _dot);
            TryGetComponent(out _penetration);
            TryGetComponent(out _despawn);
            Assert.IsNotNull(_despawn, "LinearProjectile requires a ProjectileDespawn capability.");
        }

        private void OnCollisionEnter(Collision collision) {
            if (!NetworkObject.IsSpawned || !IsOwner) {
                return;
            }
            var contact = collision.GetContact(0);
            var hitCollider = contact.otherCollider;
            var canDamage = hitCollider.TryGetComponent<Damageable>(out var damageable) && damageable.CanTakeDamage(_attacker);
            _damage?.Apply(_attacker, hitCollider, contact.point, _damageValue);
            _explosion?.Detonate(_attacker, contact.point, _damageValue);
            if (_dot != null && canDamage) {
                _dot.Apply(_attacker, damageable.Entity, _damageValue);
            }
            impactEffect?.SpawnRpc(contact.point, Quaternion.LookRotation(contact.normal));
            if (_penetration != null && _penetration.RecordHit()) {
                return;
            }
            _despawn.Request();
        }
    }
}
