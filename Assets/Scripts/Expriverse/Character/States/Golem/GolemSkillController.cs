using Expriverse.Character.Components;
using Expriverse.Combat;
using Expriverse.Health;
using Unity.Netcode;
using UnityEngine;
using VContainer;
using Xoderony.InputChannels;

namespace Expriverse.Character.States.Golem {

    public class GolemSkillController : MonoBehaviour, IComponent {
        public SphereCollider hitBox;
        public LayerMask hitMask;
        [Min(0)] public float damageMultiplier = 1;
        [Min(0)] public float force = 50;
        public AnimationCurve damageFalloff = AnimationCurve.Linear(0, 1, 1, 0);
        public AnimationCurve forceFalloff = AnimationCurve.EaseInOut(0, 1, 1, 0);

        public HealthChangeFlag damageFlags;

        [Inject, Key(Constants.Stats.AttackPower)]
        internal Stat attackPowerStat;

        [Inject] internal NetworkManager networkManager;
        [Inject] internal GolemAnimationEventDispatcher eventDispatcher;
        [Inject] internal CharacterAimInputHandler aimInputHandler;
        [Inject] internal Rigidbody body;
        [Inject] internal Entity attacker;
        [Inject] internal Entity entity;
        [Inject] internal CombatDamage combatDamage;
        private Animator _animator;
        private InputChannel<bool> _primarySkillInput;

        [Inject]
        internal void Inject(InputChannelHub inputChannelHub, Animator animator) {
            _animator = animator;
            _primarySkillInput = inputChannelHub.GetInputChannel<bool>(InputKeys.PrimarySkill);
        }

        private void OnEnable() {
            eventDispatcher.onStomp.AddListener(OnStomp);
        }

        private void Update() {
            _animator.SetBool(AnimatorHashs.isAttacking, _primarySkillInput.value);
            if (_primarySkillInput.value) {
                aimInputHandler.aimTime = 3;
            }
        }

        private void OnDisable() {
            _animator.SetBool(AnimatorHashs.isAttacking, false);
            eventDispatcher.onStomp.RemoveListener(OnStomp);
        }

        private void OnStomp(AnimationEvent arg) {
            var hitTransform = hitBox.transform;
            combatDamage.ApplySphere(
                attacker,
                hitTransform.position,
                hitBox.radius,
                hitMask,
                QueryTriggerInteraction.Ignore,
                attackPowerStat.Value * damageMultiplier,
                damageFlags,
                damageFalloff,
                broadcast: true);
        }
    }
}
