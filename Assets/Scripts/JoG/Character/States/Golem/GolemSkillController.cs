using JoG.Character.Components;
using JoG.Character.InputBanks;
using JoG.Health;
using System.Buffers;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using VContainer;
using Xoderony.Extensions;

namespace JoG.Character.States.Golem {

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
        private readonly HashSet<Entity> _hits = new();
        private Animator _animator;
        private PrimarySkillInputBank _primarySkillInput;

        [Inject]
        internal void Inject(InputBankHub inputBankHub, Animator animator) {
            _animator = animator;
            _primarySkillInput = inputBankHub.GetInputBank<PrimarySkillInputBank>();
        }

        private void OnEnable() {
            eventDispatcher.onStomp.AddListener(OnStomp);
        }

        private void Update() {
            _animator.SetBool(AnimatorHashs.isAttacking, _primarySkillInput.Value);
            if (_primarySkillInput.Value) {
                aimInputHandler.aimTime = 3;
            }
        }

        private void OnDisable() {
            _animator.SetBool(AnimatorHashs.isAttacking, false);
            eventDispatcher.onStomp.RemoveListener(OnStomp);
        }

        private void OnStomp(AnimationEvent arg) {
            var buffer = ArrayPool<Collider>.Shared.Rent(256);
            var hitTransform = hitBox.transform;
            var sourcePosition = hitTransform.position;
            var hitRadius = hitBox.radius;
            var count = Physics.OverlapSphereNonAlloc(sourcePosition, hitRadius, buffer, hitMask, QueryTriggerInteraction.Ignore);
            foreach (ref readonly var hit in buffer.AsReadOnlySpan(0, count)) {
                if (hit.TryGetComponent<Damageable>(out var damageable) && _hits.Add(damageable.Entity) && damageable.CanTakeDamage(attacker)) {
                    var hitPoint = hit.ClosestPoint(sourcePosition);
                    var hitDistance = sourcePosition.DistanceTo(hitPoint);
                    var falloffValue = damageFalloff.Evaluate(hitDistance / hitRadius);
                    var damageValue = (int)(attackPowerStat.Value * damageMultiplier * falloffValue);
                    var impulse = force * falloffValue * (hitPoint - sourcePosition).normalized;
                    var message = new HealthChangeMessage() {
                        Position = hitPoint,
                        //impulse = impulse,
                        Value = damageValue,
                        Flags = damageFlags,
                    };
                    damageable.TakeDamage(ref message, attacker);
                }
            }
            ArrayPool<Collider>.Shared.Return(buffer);
            _hits.Clear();
        }
    }
}
