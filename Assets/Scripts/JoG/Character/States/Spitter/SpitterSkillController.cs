using JoG.Character.Components;
using JoG.Character.InputBanks;
using JoG.Networking;
using JoG.Projectiles;
using Unity.Netcode;
using UnityEngine;
using VContainer;
using Xoderony.YooAsset;

namespace JoG.Character.States.Spitter {

    public class SpitterSkillController : MonoBehaviour, IComponent {
        public YooAssetReference<GameObject> projectileReference;
        public Transform muzzle;
        [Min(0)] public float damageMultiplier = 1;

        [Inject, Key(Constants.Stats.AttackPower)]
        internal Stat attackPowerStat;

        [Inject] internal NetworkObjectFactory networkObjectFactory;
        [Inject] internal SpitterAnimationEventDispatcher eventDispatcher;
        [Inject] internal CharacterAimInputHandler aimInputHandler;
        [Inject] internal Rigidbody body;
        [Inject] internal Entity attacker;
        [Inject] internal Entity entity;
        private Animator _animator;
        private PrimarySkillInputBank _primarySkillInput;
        private AimInputBank _aimInput;
        private NetworkObject _networkPrefab;

        [Inject]
        internal void Inject(InputBankHub inputBankHub, Animator animator) {
            _animator = animator;
            _primarySkillInput = inputBankHub.GetInputBank<PrimarySkillInputBank>();
            _aimInput = inputBankHub.GetInputBank<AimInputBank>();
        }

        private void Awake() {
            projectileReference.Load();
            _networkPrefab = projectileReference.AssetObject.GetComponent<NetworkObject>();
        }

        private void OnEnable() {
            eventDispatcher.onShoot.AddListener(OnShoot);
        }

        private void Update() {
            _animator.SetBool(AnimatorHashs.isAttacking, _primarySkillInput.Value);
            if (_primarySkillInput.Value) {
                aimInputHandler.aimTime = 3;
            }
        }

        private void OnDisable() {
            _animator.SetBool(AnimatorHashs.isAttacking, false);
            eventDispatcher.onShoot.RemoveListener(OnShoot);
        }

        private void OnDestroy() {
            projectileReference.Unload();
        }

        private void OnShoot(AnimationEvent arg) {
            var position = muzzle.position;
            var networkPrefab = _networkPrefab;
            var up = body.rotation * Vector3.up;
            var rotation = Quaternion.LookRotation(_aimInput.vector3 - position, up);
            var projectile = networkObjectFactory.Instantiate(networkPrefab, position: position, rotation: rotation);
            projectile.GetComponent<ProjectileEntity>()
                   .SetOwner(entity)
                   .SetProperty(Properties.Attacker, attacker)
                   .SetProperty(Properties.DamageValue, attackPowerStat.Value * damageMultiplier)
                   .SetProperty(Properties.IgnoreColliders, entity.Colliders)
                   .SetProperty(Properties.InheritedVelocity, body.GetPointVelocity(position));
            projectile.Spawn(true);
        }
    }
}
