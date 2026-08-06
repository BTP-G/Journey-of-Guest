using Xoderony.GameplayEffects;
using JoG.Gameplay.Effects;
using JoG.Gameplay.Effects.Data;
using JoG.Health;
using Xoderony.Collections;
using Xoderony.Unity;
using System;
using Unity.Netcode;
using UnityEngine.PlayerLoop;
using VContainer;

namespace JoG.Character {

    public sealed class CharacterPeriodicHealthChanges : NetworkBehaviour, IComponent {

        [Inject] internal CharacterEntity owner;

        [Inject] internal CharacterEffects effects;

        [Inject] internal CharacterModel model;

        [Inject] internal HealthChangeRouter healthChangeRouter;

        private readonly ArrayList<State> _states = new();

        public int Count => _states.Count;

        public override void OnNetworkSpawn() {
            base.OnNetworkSpawn();
            PostUpdateLoop<FixedUpdate.ScriptRunDelayedFixedFrameRate>.Register(OnPostDelayedFixedUpdate);
        }

        public void AddEffect(PeriodicHealthChangeDefinition definition, Entity source, int tickCount, int tickValue) {
            ValidateAdd(definition, tickCount);
            var nextTickAt = NetworkManager.ServerTime.Time + definition.TickInterval;
            AddEffectLocal(definition, source, tickCount, tickValue, nextTickAt);
        }

        public void AddEffectRpc(PeriodicHealthChangeDefinition definition, Entity source, int tickCount, int tickValue) {
            ValidateAdd(definition, tickCount);
            var nextTickAt = NetworkManager.ServerTime.Time + definition.TickInterval;
            ApplyAddEffectRpc(definition.Id, source, tickCount, tickValue, nextTickAt);
        }

        public void RemoveEffect(int definitionId) {
            RemoveEffectLocal(definitionId);
        }

        [Rpc(SendTo.Everyone)]
        public void RemoveEffectRpc(int definitionId) {
            RemoveEffectLocal(definitionId);
        }

        public override void OnNetworkDespawn() {
            PostUpdateLoop<FixedUpdate.ScriptRunDelayedFixedFrameRate>.Unregister(OnPostDelayedFixedUpdate);
            _states.Clear();
            base.OnNetworkDespawn();
        }

        protected override void OnSynchronize<T>(ref BufferSerializer<T> serializer) {
            base.OnSynchronize(ref serializer);
            if (serializer.IsWriter) {
                var writer = serializer.GetFastBufferWriter();
                writer.WriteValueSafe(_states.Count);
                foreach (ref var state in _states) {
                    writer.WriteValueSafe(state.Definition.Id);
                    writer.WriteValueSafe(state.Source);
                    writer.WriteValueSafe(state.RemainingTickCount);
                    writer.WriteValueSafe(state.TickValue);
                    writer.WriteValueSafe(state.NextTickAt);
                }
            } else {
                _states.Clear();
                var reader = serializer.GetFastBufferReader();
                reader.ReadValueSafe(out int stateCount);
                for (var i = 0; i < stateCount; i++) {
                    reader.ReadValueSafe(out int definitionId);
                    reader.ReadValueSafe(out Entity source);
                    reader.ReadValueSafe(out int remainingTickCount);
                    reader.ReadValueSafe(out int tickValue);
                    reader.ReadValueSafe(out double nextTickAt);
                    var definition = PeriodicHealthChangeDefinitionDictionary.Shared[definitionId];
                    _states.Add(new State(definition, source, remainingTickCount, tickValue, nextTickAt));
                }
            }
        }

        private void OnPostDelayedFixedUpdate() {
            var currentTime = NetworkManager.ServerTime.Time;
            for (var i = _states.Count - 1; i >= 0; i--) {
                ref var state = ref _states[i];
                while (state.RemainingTickCount > 0 && currentTime >= state.NextTickAt) {
                    ApplyTick(in state);
                    state.RemainingTickCount--;
                    state.NextTickAt += state.Definition.TickInterval;
                    effects.RemoveEffect(state.Definition.DisplayEffectDefinition.Id);
                }

                if (state.RemainingTickCount == 0) {
                    _states.RemoveAt(i);
                }
            }
        }

        [Rpc(SendTo.Everyone)]
        private void ApplyAddEffectRpc(int definitionId, Entity source, int tickCount, int tickValue, double nextTickAt) {
            var definition = PeriodicHealthChangeDefinitionDictionary.Shared[definitionId];
            ValidateAdd(definition, tickCount);
            AddEffectLocal(definition, source, tickCount, tickValue, nextTickAt);
        }

        private void AddEffectLocal(PeriodicHealthChangeDefinition definition, Entity source, int tickCount, int tickValue, double nextTickAt) {
            foreach (ref var state in _states) {
                if (state.Definition.Id != definition.Id || state.Source != source || (state.TickValue ^ tickValue) < 0) {
                    continue;
                }

                var previousTickCount = state.RemainingTickCount;
                state.RemainingTickCount = MergeValue(previousTickCount, tickCount, definition.TickCountMergeMode);
                state.TickValue = MergeValue(state.TickValue, tickValue, definition.TickValueMergeMode);
                ApplyEffectCountDelta(definition.DisplayEffectDefinition, state.RemainingTickCount - previousTickCount);
                return;
            }

            _states.Add(new State(definition, source, tickCount, tickValue, nextTickAt));
            effects.AddEffect(definition.DisplayEffectDefinition, tickCount);
        }

        private void RemoveEffectLocal(int definitionId) {
            for (var i = _states.Count - 1; i >= 0; i--) {
                ref var state = ref _states[i];
                if (state.Definition.Id != definitionId) {
                    continue;
                }

                effects.RemoveEffect(state.Definition.DisplayEffectDefinition.Id, state.RemainingTickCount);
                _states.RemoveAt(i);
            }
        }

        private void ApplyEffectCountDelta(GameplayEffectDefinition definition, int countDelta) {
            if (countDelta > 0) {
                effects.AddEffect(definition, countDelta);
            } else if (countDelta < 0) {
                effects.RemoveEffect(definition.Id, -countDelta);
            }
        }

        private void ApplyTick(in State state) {
            var message = new HealthChangeMessage {
                Value = state.TickValue,
                Flags = state.Definition.HealthChangeFlags,
                Color = state.Definition.Color,
                Position = model.Center,
            };
            healthChangeRouter.Route(state.Source, owner, ref message);
        }

        private static int MergeValue(int currentValue, int newValue, MergeMode mergeMode) {
            return mergeMode switch {
                MergeMode.None => currentValue,
                MergeMode.Overwrite => newValue,
                MergeMode.Additive => currentValue + newValue,
                MergeMode.Average => (currentValue + newValue) / 2,
                MergeMode.Minimum => Math.Min(currentValue, newValue),
                MergeMode.Maximum => Math.Max(currentValue, newValue),
                _ => currentValue,
            };
        }

        private static void ValidateAdd(PeriodicHealthChangeDefinition definition, int tickCount) {
            if (definition == null) {
                throw new ArgumentNullException(nameof(definition));
            }
            if (definition.Id == 0) {
                throw new InvalidOperationException($"{definition.name} is not registered.");
            }
            if (definition.DisplayEffectDefinition == null) {
                throw new InvalidOperationException($"{definition.name} does not reference a display GameplayEffectDefinition.");
            }
            if (definition.DisplayEffectDefinition.Id == 0) {
                throw new InvalidOperationException($"{definition.DisplayEffectDefinition.name} is not registered.");
            }
            if (tickCount <= 0) {
                throw new ArgumentOutOfRangeException(nameof(tickCount), tickCount, "Tick count must be greater than zero.");
            }
        }

        private struct State {

            public PeriodicHealthChangeDefinition Definition;

            public Entity Source;

            public int RemainingTickCount;

            public int TickValue;

            public double NextTickAt;

            public State(PeriodicHealthChangeDefinition definition, Entity source, int remainingTickCount, int tickValue, double nextTickAt) {
                Definition = definition;
                Source = source;
                RemainingTickCount = remainingTickCount;
                TickValue = tickValue;
                NextTickAt = nextTickAt;
            }
        }
    }
}
