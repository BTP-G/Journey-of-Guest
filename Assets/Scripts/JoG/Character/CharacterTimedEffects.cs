using Xoderony.GameplayEffects;
using Xoderony.Collections;
using Xoderony.Unity;
using System;
using Unity.Netcode;
using UnityEngine.PlayerLoop;
using VContainer;

namespace JoG.Character {

    public sealed class CharacterTimedEffects : NetworkBehaviour, IComponent {

        [Inject] internal CharacterEffects effects;

        private readonly ArrayList<State> _states = new();

        public int Count => _states.Count;

        public override void OnNetworkSpawn() {
            base.OnNetworkSpawn();
            PostUpdateLoop<FixedUpdate.ScriptRunDelayedFixedFrameRate>.Register(OnPostDelayedFixedUpdate);
        }

        /// <param name="duration">持续时间，单位为秒。</param>
        public void AddEffect(GameplayEffectDefinition definition, float duration, int count = 1) {
            ValidateAdd(definition, duration, count);
            var expiresAt = NetworkManager.ServerTime.Time + duration;
            AddEffectLocal(definition, count, expiresAt);
        }

        /// <param name="duration">持续时间，单位为秒。</param>
        public void AddEffectRpc(GameplayEffectDefinition definition, float duration, int count = 1) {
            ValidateAdd(definition, duration, count);
            var expiresAt = NetworkManager.ServerTime.Time + duration;
            ApplyAddEffectRpc(definition.Id, count, expiresAt);
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
                    writer.WriteValueSafe(state.Count);
                    writer.WriteValueSafe(state.ExpiresAt);
                }
            } else {
                _states.Clear();
                var reader = serializer.GetFastBufferReader();
                reader.ReadValueSafe(out int stateCount);
                for (var i = 0; i < stateCount; i++) {
                    reader.ReadValueSafe(out int definitionId);
                    reader.ReadValueSafe(out int count);
                    reader.ReadValueSafe(out double expiresAt);
                    var definition = GameplayEffectDefinitionRegistry.Shared[definitionId];
                    _states.Add(new State(definition, count, expiresAt));
                }
            }
        }

        private void OnPostDelayedFixedUpdate() {
            var currentTime = NetworkManager.ServerTime.Time;
            for (var i = _states.Count - 1; i >= 0; i--) {
                ref var state = ref _states[i];
                if (currentTime < state.ExpiresAt) {
                    continue;
                }
                effects.RemoveEffect(state.Definition.Id, state.Count);
                _states.RemoveAt(i);
            }
        }

        [Rpc(SendTo.Everyone)]
        private void ApplyAddEffectRpc(int definitionId, int count, double expiresAt) {
            var definition = GameplayEffectDefinitionRegistry.Shared[definitionId];
            AddEffectLocal(definition, count, expiresAt);
        }

        private void AddEffectLocal(GameplayEffectDefinition definition, int count, double expiresAt) {
            _states.Add(new State(definition, count, expiresAt));
            effects.AddEffect(definition, count);
        }

        private void RemoveEffectLocal(int definitionId) {
            for (var i = _states.Count - 1; i >= 0; i--) {
                ref var state = ref _states[i];
                if (state.Definition.Id != definitionId) {
                    continue;
                }
                effects.RemoveEffect(definitionId, state.Count);
                _states.RemoveAt(i);
            }
        }

        private static void ValidateAdd(GameplayEffectDefinition definition, float duration, int count) {
            if (definition == null) {
                throw new ArgumentNullException(nameof(definition));
            }
            if (definition.Id == 0) {
                throw new InvalidOperationException($"{definition.name} is not registered.");
            }
            if (duration <= 0f) {
                throw new ArgumentOutOfRangeException(nameof(duration), duration, "Duration must be greater than zero.");
            }
            if (count <= 0) {
                throw new ArgumentOutOfRangeException(nameof(count), count, "Effect count must be greater than zero.");
            }
        }

        private struct State {

            public GameplayEffectDefinition Definition;

            public int Count;

            public double ExpiresAt;

            public State(GameplayEffectDefinition definition, int count, double expiresAt) {
                Definition = definition;
                Count = count;
                ExpiresAt = expiresAt;
            }
        }
    }
}
