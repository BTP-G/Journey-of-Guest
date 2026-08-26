using Expriverse.Character;
using Expriverse.GameplayEffects.Data;
using Expriverse.Health;
using System;
using System.Runtime.CompilerServices;
using VContainer;
using Xoderony;
using Xoderony.Collections;
using Xoderony.GameplayEffects;
using Xoderony.YooAsset;

namespace Expriverse.GameplayEffects.Controllers {

    [Serializable]
    public sealed class DamageDealtPeriodicDamageController : GameplayEffectController<DamageDealtPeriodicDamageData>, IComponent {

        [Inject] internal Entity owner;

        private readonly ArrayList<EffectState> _states = new();

        [Inject]
        internal void Subscribe(IDelegateSubscriber<OutgoingDamageReportHandler> outgoingDamageReports) {
            outgoingDamageReports.Subscribe(OnOutgoingDamageReport);
        }

        protected override void SetEffectCount(int definitionId, DamageDealtPeriodicDamageData data, int count) {
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

                var definition = GetDefinition(ref data.Definition);
                if (definition == null) {
                    continue;
                }

                var tickValue = DamageEffectUtility.CalculateDamage(data.TickDamage, data.ActualDamageMultiplier, 1, report);
                if (tickValue == 0) {
                    continue;
                }

                var tickCount = state.Count * data.TickCountPerStack;
                target.PeriodicHealthChanges.AddEffect(definition, report.source ?? owner, tickCount, tickValue);
            }
        }

        private static PeriodicHealthChangeDefinition GetDefinition(ref YooAssetReference<PeriodicHealthChangeDefinition> reference) {
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

            public DamageDealtPeriodicDamageData Data;

            public int Count;
        }
    }
}
