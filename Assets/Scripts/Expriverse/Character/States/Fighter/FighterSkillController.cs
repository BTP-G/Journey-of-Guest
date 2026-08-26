using EditorAttributes;
using Expriverse.Character.Components;
using Expriverse.Health;
using Expriverse.Networking.Components;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using Xoderony.Numerics;
using Xoderony.InputChannels;

namespace Expriverse.Character.States.Fighter {

    public class FighterSkillController : MonoBehaviour, IComponent {

        [Required]
        public HitBox swordHitBox;

        [Required]
        public HitBox shieldHitBox;

        [Required]
        public NetworkEffectSpawner swordHitEffectSpawner;

        [Required]
        public NetworkEffectSpawner shieldHitEffectSpawner;

        [Tooltip("text")]
        public Q16 swordDamageMultiplier = new Q16(1f);

        [Min(0)]
        public float shieldDamageMultiplier = 1;

        [Min(0)]
        public float swordDamageForce = 30;

        [Min(0)]
        public float shieldDamageForce = 30;

        public HealthChangeFlag damageFlags;

        public Color32 damageColor = Color.white;

        [Inject]
        [Key(Constants.Stats.AttackPower)]
        internal Stat attackPowerStat;

        [Inject]
        internal FighterAnimationEventDispatcher eventDispatcher;

        [Inject]
        internal CharacterAimInputHandler aimInputHandler;

        [Inject]
        internal Entity attacker;

        [Inject]
        internal Rigidbody body;

        private readonly HashSet<Entity> _hits = new();

        private Animator _animator;

        private InputChannel<bool> _primarySkillInput;

        private InputChannel<bool> _secondarySkillInput;

        [Inject]
        internal void Inject(InputChannelHub inputChannelHub, Animator animator) {
            _animator = animator;
            _primarySkillInput = inputChannelHub.GetInputChannel<bool>(InputKeys.PrimarySkill);
            _secondarySkillInput = inputChannelHub.GetInputChannel<bool>(InputKeys.SecondarySkill);
        }

        private void OnEnable() {
            eventDispatcher.onSwordSwing
                           .AddListener(OnSwordSwing);
            eventDispatcher.onShieldSwing
                           .AddListener(OnShieldSwing);
            swordHitBox.onHit
                       .AddListener(OnSwordHit);
            shieldHitBox.onHit
                        .AddListener(OnShieldHit);
            swordHitBox.Disable();
            shieldHitBox.Disable();
        }

        private void Update() {
            _animator.SetBool(AnimatorHashs.isAttackingR, _primarySkillInput.value);
            _animator.SetBool(AnimatorHashs.isAttackingL, _secondarySkillInput.value);
            if (_primarySkillInput.value || _secondarySkillInput.value) {
                aimInputHandler.aimTime = 3;
            }
        }

        private void OnDisable() {
            _animator.SetBool(AnimatorHashs.isAttackingL, false);
            _animator.SetBool(AnimatorHashs.isAttackingR, false);
            eventDispatcher.onSwordSwing
                           .RemoveListener(OnSwordSwing);
            eventDispatcher.onShieldSwing
                           .RemoveListener(OnShieldSwing);
            swordHitBox.onHit
                       .RemoveListener(OnSwordHit);
            shieldHitBox.onHit
                        .RemoveListener(OnShieldHit);
            swordHitBox.Disable();
            shieldHitBox.Disable();
        }

        private void OnSwordSwing(AnimationEvent arg) {
            _hits.Clear();
            swordHitBox.Activate(arg.floatParameter);
        }

        private void OnShieldSwing(AnimationEvent arg) {
            _hits.Clear();
            shieldHitBox.Activate(arg.floatParameter);
        }

        private void OnSwordHit(Collider collider) {
            if (!attacker.HasAuthority) {
                return;
            }
            if (collider.TryGetComponent<HurtBox>(out var hurtBox) && _hits.Add(hurtBox.Entity)) {
                var hitPoint = collider.bounds.center;
                if (collider.TryGetComponent<IHittable>(out var hittable)) {
                    var hitMessage = new HitMessage {
                        point = hitPoint,
                        impulse = body.rotation
                            * new Vector3(
                                0,
                                0.707f,
                                0.707f
                            )
                            * swordDamageForce,
                    };
                    hittable.TakeHit(hitMessage, attacker);
                }
                if (collider.TryGetComponent<IDamageable>(out var damageable) && damageable.CanTakeDamage(attacker)) {
                    var message = new HealthChangeMessage {
                        Flags = damageFlags,
                        Color = damageColor,
                        Value = attackPowerStat.Value * swordDamageMultiplier,
                        Position = hitPoint,
                    };
                    damageable.TakeDamage(ref message, attacker);
                }
                swordHitEffectSpawner.SpawnRpc(hitPoint, Random.rotationUniform);
            }
        }

        private void OnShieldHit(Collider collider) {
            if (!attacker.HasAuthority) {
                return;
            }
            if (collider.TryGetComponent<HurtBox>(out var hurtBox) && _hits.Add(hurtBox.Entity)) {
                var hitPoint = collider.bounds.center;
                if (collider.TryGetComponent<IHittable>(out var hittable)) {
                    var hitMessage = new HitMessage {
                        point = hitPoint,
                        impulse = body.rotation
                            * new Vector3(
                                0,
                                0.707f,
                                0.707f
                            )
                            * shieldDamageForce,
                    };
                    hittable.TakeHit(hitMessage, attacker);
                }
                if (collider.TryGetComponent<IDamageable>(out var damageable) && damageable.CanTakeDamage(attacker)) {
                    var message = new HealthChangeMessage {
                        Flags = damageFlags,
                        Color = damageColor,
                        Value = (int)(attackPowerStat.Value * shieldDamageMultiplier),
                        Position = hitPoint,
                    };
                    damageable.TakeDamage(ref message, attacker);
                }
                shieldHitEffectSpawner.SpawnRpc(hitPoint, Random.rotationUniform);
            }
        }
    }
}
