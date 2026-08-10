using JoG.Character.Components;
using JoG.Networking;
using JoG.Projectiles;
using Unity.Netcode;
using UnityEngine;
using VContainer;
using Xoderony.YooAsset;
using Xoderony.InputChannels;

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
        private InputChannel<bool> _primarySkillInput;
        private InputChannel<AimInput> _aimInput;
        private NetworkObject _networkPrefab;

        [Inject]
        internal void Inject(InputChannelHub inputChannelHub, Animator animator) {
            _animator = animator;
            _primarySkillInput = inputChannelHub.GetInputChannel<bool>(InputKeys.PrimarySkill);
            _aimInput = inputChannelHub.GetInputChannel<AimInput>(InputKeys.Aim);
        }

        private void Awake() {
            projectileReference.Load();
            _networkPrefab = projectileReference.AssetObject.GetComponent<NetworkObject>();
        }

        private void OnEnable() {
            eventDispatcher.onShoot.AddListener(OnShoot);
        }

        private void Update() {
            _animator.SetBool(AnimatorHashs.isAttacking, _primarySkillInput.value);
            if (_primarySkillInput.value) {
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
            var rotation = Quaternion.LookRotation(_aimInput.value.position - position, up);
            var projectile = networkObjectFactory.Instantiate(networkPrefab, position: position, rotation: rotation);
            projectile.GetComponent<LinearProjectile>()
                   .Initialize(entity, attacker, attackPowerStat.Value * damageMultiplier, body.GetPointVelocity(position), entity.Colliders);
            projectile.Spawn(true);
        }
    }
}
