using EditorAttributes;
using Xoderony;
using Xoderony.YooAsset;
using JoG.Character.Components;
using JoG.Character.InputBanks;
using JoG.Networking;
using JoG.Networking.Components;
using JoG.Projectiles;
using System;
using Unity.Netcode;
using UnityEngine;
using VContainer;

namespace JoG.Character.States.Mage {

    public class MageSkillController : MonoBehaviour, IComponent {
        public Spell leftSpell;
        public Spell rightSpell;
        public Spell upSpell;

        [Inject, Key(Constants.Stats.AttackPower)]
        internal Stat attackPowerStat;

        [Inject] internal Rigidbody body;
        [Inject] internal Animator animator;
        [Inject] internal NetworkObjectFactory networkObjectFactory;
        [Inject] internal Entity entity;
        [Inject] internal Entity attacker;
        [Inject] internal CharacterAimInputHandler aimInputHander;

        private PrimarySkillInputBank _primarySkillInput;
        private SecondarySkillInputBank _secondarySkillInput;
        private AimInputBank _aimInput;

        private IDelegateSubscriber<AnimationEventHandler> _animationEvents;

        [Inject]
        internal void Inject(
            InputBankHub inputBankHub,
            IDelegateSubscriber<AnimationEventHandler> animationEvents) {

            _aimInput = inputBankHub.GetInputBank<AimInputBank>();
            _primarySkillInput = inputBankHub.GetInputBank<PrimarySkillInputBank>();
            _secondarySkillInput = inputBankHub.GetInputBank<SecondarySkillInputBank>();
            _animationEvents = animationEvents;
        }

        private void Awake() {
            leftSpell.LoadPrefab();
            rightSpell.LoadPrefab();
            upSpell.LoadPrefab();
        }

        private void OnEnable() {
            _animationEvents.Subscribe(HandleAnimationEvent);
        }

        private void Update() {
            animator.SetBool(AnimatorHashs.isChargingL, _primarySkillInput.Value);
            animator.SetBool(AnimatorHashs.isChargingR, _secondarySkillInput.Value);
        }

        private void OnDisable() {
            _animationEvents.Unsubscribe(HandleAnimationEvent);
        }

        private void OnDestroy() {
            leftSpell.UnloadPrefab();
            rightSpell.UnloadPrefab();
            upSpell.UnloadPrefab();
        }

        private void HandleAnimationEvent(in AnimationEvent animationEvent) {
            aimInputHander.aimTime = 9999;
            switch (animationEvent.intParameter) {
                case 1:
                    leftSpell.particleSystem.Play();
                    break;

                case 2:
                    aimInputHander.aimTime = 3;
                    leftSpell.particleSystem.Stop();
                    FireProjectile(leftSpell);
                    break;

                case 3:
                    rightSpell.particleSystem.Play();
                    break;

                case 4:
                    aimInputHander.aimTime = 3;
                    rightSpell.particleSystem.Stop();
                    FireProjectile(rightSpell);
                    break;

                case 5:
                    leftSpell.particleSystem.Stop();
                    rightSpell.particleSystem.Stop();
                    upSpell.particleSystem.Play();
                    break;

                case 6:
                    aimInputHander.aimTime = 3;
                    upSpell.particleSystem.Stop();
                    CallProjectile(upSpell);
                    break;
            }
        }

        private void FireProjectile(in Spell spell) {
            var position = spell.muzzle.position;
            var networkPrefab = spell.networkPrefabCache;
            var up = body.rotation * Vector3.up;
            var rotation = Quaternion.LookRotation(_aimInput.vector3 - position, up);
            var projectile = networkObjectFactory.Instantiate(networkPrefab, position: position, rotation: rotation);
            projectile.GetComponent<ProjectileEntity>()
                   .SetOwner(entity)
                   .SetProperty(Properties.Attacker, attacker)
                   .SetProperty(Properties.DamageValue, attackPowerStat.Value * spell.damageMultiplier)
                   .SetProperty(Properties.IgnoreColliders, entity.Colliders)
                   .SetProperty(Properties.InheritedVelocity, body.GetPointVelocity(position));
            projectile.Spawn(true);
        }

        private void CallProjectile(in Spell spell) {
            var mask = LayerMasks.Default | LayerMasks.Prop;
            var origin = spell.muzzle.position;
            var direction = _aimInput.vector3 - origin;
            if (Physics.Raycast(origin, direction, out var hitInfo, 300, mask, QueryTriggerInteraction.Ignore)) {
                var projectile = networkObjectFactory.Instantiate(
                    spell.networkPrefabCache,
                    position: hitInfo.point,
                    rotation: body.rotation);
                projectile.GetComponent<ProjectileEntity>()
                       .SetOwner(entity)
                       .SetProperty(Properties.Attacker, attacker)
                       .SetProperty(Properties.DamageValue, attackPowerStat.Value * spell.damageMultiplier);
                projectile.Spawn(true);
            }
        }

        [Serializable]
        public struct Spell {
            public YooAssetReference<GameObject> prefab;
            [Required] public NetworkParticleSystem particleSystem;
            [Required] public Transform muzzle;
            [Min(0)] public float damageMultiplier;
            [NonSerialized] public NetworkObject networkPrefabCache;

            public void LoadPrefab() {
                prefab.Load();
                networkPrefabCache = prefab.AssetObject.GetComponent<NetworkObject>();
            }

            public void UnloadPrefab() {
                networkPrefabCache = null;
                prefab.Unload();
            }
        }
    }
}
