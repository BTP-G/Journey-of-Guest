using JoG.Health;
using EditorAttributes;
using JoG.Character.Components;
using JoG.Character.InputBanks;
using JoG.Networking.Components;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

namespace JoG.Character.States.Skeleton {

    public class SkeletonSkillController : MonoBehaviour, IComponent {
        [Required] public HitBox hitBox;
        [Required] public NetworkEffectSpawner hitSpawnEffectSpawner;

        [Min(0)] public float damageMultiplier = 1;

        public HealthChangeFlag damageFlags;

        [Inject, Key(Constants.Stats.AttackPower)]
        internal Stat attackPowerStat;

        [Inject] internal SkeletonAnimationEventDispatcher eventDispatcher;
        [Inject] internal CharacterAimInputHandler aimInputHandler;
        [Inject] internal Entity attacker;
        private readonly HashSet<Entity> _hits = new();
        private Animator _animator;
        private PrimarySkillInputBank _primarySkillInput;

        [Inject]
        internal void Inject(InputBankHub inputBankHub, Animator animator) {
            _animator = animator;
            _primarySkillInput = inputBankHub.GetInputBank<PrimarySkillInputBank>();
        }

        private void OnEnable() {
            eventDispatcher.onSwordSwing.AddListener(OnSwordSwing);
            hitBox.onHit.AddListener(OnSwordHit);
            hitBox.Disable();
        }

        private void Update() {
            _animator.SetBool(AnimatorHashs.isAttacking, _primarySkillInput.Value);
            if (_primarySkillInput.Value) {
                aimInputHandler.aimTime = 3;
            }
        }

        private void OnDisable() {
            _animator.SetBool(AnimatorHashs.isAttacking, false);
            eventDispatcher.onSwordSwing.RemoveListener(OnSwordSwing);
            hitBox.onHit.RemoveListener(OnSwordHit);
            hitBox.Disable();
        }

        private void OnSwordSwing(AnimationEvent arg) {
            _hits.Clear();
            hitBox.Activate(arg.floatParameter);
        }

        private void OnSwordHit(Collider collider) {
            if (collider.TryGetComponent(out Damageable damageable) && _hits.Add(damageable.Entity) && damageable.CanTakeDamage(attacker)) {
                var message = new HealthChangeMessage {
                    Value = (int)(attackPowerStat.Value * damageMultiplier),
                    Flags = damageFlags,
                    Position = collider.bounds.center,
                };
                damageable.TakeDamage(ref message, attacker);
                hitSpawnEffectSpawner.SpawnRpc(message.Position, Random.rotationUniform);
            }
        }
    }
}
