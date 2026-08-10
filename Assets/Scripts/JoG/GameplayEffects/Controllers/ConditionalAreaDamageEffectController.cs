using JoG.Character;
using JoG.Combat;
using JoG.GameplayEffects.Data;
using JoG.Health;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using VContainer;
using Xoderony;
using Xoderony.Collections;
using Xoderony.Extensions;
using Xoderony.GameplayEffects;
using Xoderony.ObjectPool.Unity;
using Xoderony.Unity;
using Xoderony.YooAsset;
using UObject = UnityEngine.Object;

namespace JoG.GameplayEffects.Controllers {

    [Serializable]
    public sealed class ConditionalAreaDamageEffectController : GameplayEffectController<ConditionalAreaDamageEffectData>, IComponent {

        [Inject] internal Entity owner;

        [Inject] internal CombatDamage combatDamage;

        private readonly ArrayList<EffectState> _states = new();

        private readonly Dictionary<GameObject, GameObjectPool> _effectPools = new();

        private readonly Dictionary<GameObject, GameObjectPool> _activeEffectPools = new();

        [Inject]
        internal void Subscribe(IDelegateSubscriber<OutgoingDamageReportHandler> outgoingDamageReports) {
            outgoingDamageReports.Subscribe(OnOutgoingDamageReport);
        }

        protected override void SetEffectCount(int definitionId, ConditionalAreaDamageEffectData data, int count) {
            if (count == 0) {
                RemoveState(definitionId);
                return;
            }
            AddOrUpdate(new EffectState {
                DefinitionId = definitionId,
                Data = data,
                Count = count
            });
        }

        protected override void Clear() {
            _states.Clear();
        }

        private void OnOutgoingDamageReport(in HealthChangeReport report) {
            if (report.deltaValue >= 0 || report.target is not CharacterEntity target) {
                return;
            }

            foreach (ref var state in _states) {
                var data = state.Data;
                if (!DamageEffectUtility.MatchesFlags(report.flags, data.RequiredFlags, data.ExcludedFlags)) {
                    continue;
                }

                var requiredEffect = GetAsset(ref data.RequiredEffect);
                if (requiredEffect == null || target.Effects.GetEffectCount(requiredEffect.Id) == 0) {
                    continue;
                }

                var damage = DamageEffectUtility.CalculateDamage(data.Damage, data.ActualDamageMultiplier, state.Count, report);
                if (damage == 0) {
                    continue;
                }

                SpawnEffect(ref data.EffectPrefab, report.position, Quaternion.identity);
                DamageArea(report.source ?? owner, report.position, data.Radius, data.HitLayer, damage, data.OutputFlags);
            }
        }

        private void DamageArea(Entity source, Vector3 position, float radius, LayerMask hitLayer, int damage, HealthChangeFlag flags) {
            if (radius <= 0f) {
                return;
            }

            combatDamage.ApplySphere(source, position, radius, hitLayer, QueryTriggerInteraction.Collide, -damage, flags, null, broadcast: false);
        }

        private void SpawnEffect(ref YooAssetReference<GameObject> reference, Vector3 position, Quaternion rotation) {
            var prefab = GetAsset(ref reference);
            if (prefab == null) {
                return;
            }
            if (!_effectPools.TryGetValue(prefab, out var pool)) {
                pool = ObjectPoolManager<GameObject>.GetPool<GameObjectPool>(prefab);
                _effectPools[prefab] = pool;
            }

            var events = pool.Rent(position, rotation).GetOrAddComponent<ParticleSystemEvents>();
            _activeEffectPools[events.gameObject] = pool;
            events.ParticleSystemStopped += OnEffectStopped;
            events.gameObject.SetActive(true);
        }

        private void OnEffectStopped(ParticleSystemEvents events) {
            events.ParticleSystemStopped -= OnEffectStopped;
            events.gameObject.SetActive(false);
            if (_activeEffectPools.TryGetValue(events.gameObject, out var pool)) {
                _activeEffectPools.Remove(events.gameObject);
                pool.Return(events.gameObject);
                return;
            }
            UObject.Destroy(events.gameObject);
        }

        private static T GetAsset<T>(ref YooAssetReference<T> reference) where T : UObject {
            if (reference == null) {
                return null;
            }
            if (reference.AssetHandle is null) {
                reference.Load();
            }
            return reference.AssetHandle is null ? null : reference.AssetObject;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddOrUpdate(in EffectState newState) {
            for (var i = 0; i < _states.Count; i++) {
                ref var state = ref _states[i];
                if (state.DefinitionId != newState.DefinitionId) {
                    continue;
                }
                state = newState;
                return;
            }
            _states.Add(newState);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RemoveState(int definitionId) {
            for (var i = 0; i < _states.Count; i++) {
                if (_states[i].DefinitionId == definitionId) {
                    _states.SwapRemoveAt(i);
                    return;
                }
            }
        }

        private struct EffectState {

            public int DefinitionId;

            public ConditionalAreaDamageEffectData Data;

            public int Count;
        }
    }
}
