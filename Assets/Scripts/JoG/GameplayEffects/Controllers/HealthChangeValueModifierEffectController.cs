using JoG.GameplayEffects.Data;
using JoG.Health;
using System;
using System.Runtime.CompilerServices;
using VContainer;
using Xoderony;
using Xoderony.Collections;
using Xoderony.GameplayEffects;
using Xoderony.Numerics;

namespace JoG.GameplayEffects.Controllers {

    [Serializable]
    public abstract class HealthChangeValueModifierEffectController<TData> : GameplayEffectController<TData>, IComponent where TData : HealthChangeValueModifierEffectData {

        private readonly ArrayList<EffectState> _states = new();

        protected override void SetEffectCount(int definitionId, TData data, int count) {
            if (count == 0) {
                RemoveState(definitionId);
                return;
            }
            foreach (ref var state in _states) {
                if (state.DefinitionId != definitionId) {
                    continue;
                }
                state.Data = data;
                state.Count = count;
                return;
            }
            _states.Add(new EffectState(definitionId, data, count));
        }

        protected override void Clear() {
            _states.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void ModifyMessage(ref HealthChangeMessage message) {
            foreach (ref var state in _states) {
                var multiplier = CalculateMultiplier(state.Data.MultiplierBonus, state.Count);
                message.Value = Math.Clamp(message.Value * multiplier, HealthChangeMessage.MinValueInt, HealthChangeMessage.MaxValueInt);
            }
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

        private struct EffectState {

            public int DefinitionId;

            public TData Data;

            public int Count;

            public EffectState(int definitionId, TData data, int count) {
                DefinitionId = definitionId;
                Data = data;
                Count = count;
            }
        }
    }

    [Serializable]
    public sealed class OutgoingHealthChangeValueModifierEffectController : HealthChangeValueModifierEffectController<OutgoingHealthChangeValueModifierEffectData> {

        [Inject]
        internal void SubscribeModifiers(IDelegateSubscriber<OutgoingDamageMessageModifier> outgoingDamageModifiers, IDelegateSubscriber<OutgoingHealMessageModifier> outgoingHealModifiers) {
            outgoingDamageModifiers.Subscribe(OnOutgoingDamageMessage);
            outgoingHealModifiers.Subscribe(OnOutgoingHealMessage);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OnOutgoingDamageMessage(ref HealthChangeMessage message, in Entity target) {
            ModifyMessage(ref message);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OnOutgoingHealMessage(ref HealthChangeMessage message, Entity target) {
            ModifyMessage(ref message);
        }
    }

    [Serializable]
    public sealed class IncomingHealthChangeValueModifierEffectController : HealthChangeValueModifierEffectController<IncomingHealthChangeValueModifierEffectData> {

        [Inject]
        internal void SubscribeModifiers(IDelegateSubscriber<IncomingDamageMessageModifier> incomingDamageModifiers, IDelegateSubscriber<IncomingHealMessageModifier> incomingHealModifiers) {
            incomingDamageModifiers.Subscribe(OnIncomingDamageMessage);
            incomingHealModifiers.Subscribe(OnIncomingHealMessage);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OnIncomingDamageMessage(ref HealthChangeMessage message, in Entity attacker) {
            ModifyMessage(ref message);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OnIncomingHealMessage(ref HealthChangeMessage message, Entity healer) {
            ModifyMessage(ref message);
        }
    }
}
