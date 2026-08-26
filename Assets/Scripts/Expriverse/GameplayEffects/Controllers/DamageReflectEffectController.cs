using Expriverse.Character;
using Expriverse.GameplayEffects.Data;
using Expriverse.Health;
using System;
using System.Runtime.CompilerServices;
using VContainer;
using Xoderony;
using Xoderony.Collections;
using Xoderony.GameplayEffects;

namespace Expriverse.GameplayEffects.Controllers {

    [Serializable]
    public sealed class DamageReflectEffectController : GameplayEffectController<DamageReflectEffectData>, IComponent {

        [Inject] internal Entity owner;

        [Inject] internal HealthChangeRouter healthChangeRouter;

        private readonly ArrayList<EffectState> _states = new();

        [Inject]
        internal void Subscribe(IDelegateSubscriber<IncomingDamageReportHandler> incomingDamageReports) {
            incomingDamageReports.Subscribe(OnIncomingDamageReport);
        }

        protected override void SetEffectCount(int definitionId, DamageReflectEffectData data, int count) {
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

        private void OnIncomingDamageReport(in HealthChangeReport report) {
            if (report.source == null || report.deltaValue >= 0) {
                return;
            }

            foreach (ref var state in _states) {
                var data = state.Data;
                if (!DamageEffectUtility.MatchesFlags(report.flags, data.RequiredFlags, data.ExcludedFlags)) {
                    continue;
                }

                var value = DamageEffectUtility.CalculateDamage(0, data.ActualDamageMultiplier, state.Count, report);
                if (value == 0) {
                    continue;
                }

                var target = report.source;
                var position = target is CharacterEntity character ? character.Model.Center : target.transform.position;
                var message = new HealthChangeMessage {
                    Value = value,
                    Flags = data.OutputFlags,
                    Position = position,
                };
                healthChangeRouter.Route(owner, target, ref message);
            }
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

            public DamageReflectEffectData Data;

            public int Count;
        }
    }
}
