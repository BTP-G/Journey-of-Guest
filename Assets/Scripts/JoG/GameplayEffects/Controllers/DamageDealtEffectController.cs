using JoG.Character;
using JoG.GameplayEffects.Data;
using JoG.Health;
using System;
using System.Runtime.CompilerServices;
using VContainer;
using Xoderony;
using Xoderony.Collections;
using Xoderony.GameplayEffects;
using Xoderony.YooAsset;

namespace JoG.GameplayEffects.Controllers {

    [Serializable]
    public sealed class DamageDealtEffectController : GameplayEffectController<DamageDealtEffectData>, IComponent {

        private readonly ArrayList<EffectState> _states = new();

        [Inject]
        internal void Subscribe(IDelegateSubscriber<OutgoingDamageReportHandler> outgoingDamageReports) {
            outgoingDamageReports.Subscribe(OnOutgoingDamageReport);
        }

        protected override void SetEffectCount(int definitionId, DamageDealtEffectData data, int count) {
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

                var appliedEffect = GetDefinition(ref data.AppliedEffect);
                if (appliedEffect != null) {
                    target.Effects.AddEffect(appliedEffect, state.Count * data.CountPerStack);
                }
            }
        }

        private static GameplayEffectDefinition GetDefinition(ref YooAssetReference<GameplayEffectDefinition> reference) {
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

            public DamageDealtEffectData Data;

            public int Count;
        }
    }
}
