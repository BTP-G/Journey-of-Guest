using JoG.Character;
using JoG.GameplayEffects.Data;
using System;
using System.Runtime.CompilerServices;
using VContainer;
using Xoderony.Collections;
using Xoderony.GameplayEffects;
using Xoderony.Logging;
using Xoderony.Numerics;

namespace JoG.GameplayEffects.Controllers {

    [Serializable]
    public sealed class StatEffectController : GameplayEffectController<StatEffectData>, IComponent {

        [Inject] internal CharacterEntity owner;

        private readonly ArrayList<EffectState> _states = new();

        protected override void SetEffectCount(int definitionId, StatEffectData data, int count) {
            if (count == 0) {
                RemoveState(definitionId);
                return;
            }

            foreach (ref var state in _states) {
                if (state.DefinitionId != definitionId) {
                    continue;
                }
                var multiplier = CalculateMultiplier(data.MultiplierBonus, count);
                state.Stat.SetMultiplier(state.MultiplierSlotIndex, multiplier);
                return;
            }

            var statKey = data.StatKey;
            if (!owner.TryGetComponent<Stat>(out var stat, statKey)) {
                this.LogWarning($"Invalid stat key: {statKey}");
                return;
            }

            var newMultiplier = CalculateMultiplier(data.MultiplierBonus, count);
            var slotIndex = stat.AcquireMultiplierSlot(newMultiplier);
            _states.Add(new EffectState(definitionId, stat, slotIndex));
        }

        protected override void Clear() {
            foreach (ref var state in _states) {
                DisposeState(state);
            }
            _states.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RemoveState(int definitionId) {
            for (var i = 0; i < _states.Count; i++) {
                ref var state = ref _states[i];
                if (state.DefinitionId != definitionId) {
                    continue;
                }
                DisposeState(state);
                _states.SwapRemoveAt(i);
                return;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void DisposeState(in EffectState state) {
            state.Stat.ReleaseMultiplierSlot(state.MultiplierSlotIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Q16 CalculateMultiplier(Q16 multiplierBonus, int count) {
            var multiplier = Q16.One;
            if (multiplierBonus >= Q16.Zero) {
                for (var i = 0; i < count; i++) {
                    multiplier += multiplierBonus;
                }
                return multiplier;
            }

            var multiplierPerCount = Q16.One + multiplierBonus;
            for (var i = 0; i < count; i++) {
                multiplier *= multiplierPerCount;
            }
            return multiplier;
        }

        private readonly struct EffectState {

            public readonly int DefinitionId;

            public readonly Stat Stat;

            public readonly int MultiplierSlotIndex;

            public EffectState(int definitionId, Stat stat, int multiplierSlotIndex) {
                DefinitionId = definitionId;
                Stat = stat;
                MultiplierSlotIndex = multiplierSlotIndex;
            }
        }
    }
}
