using System;
using System.Collections.Generic;
using Unity.Netcode;
using VContainer;
using Xoderony.GameplayEffects;

namespace JoG.Character {

    public readonly struct CharacterEffectState {

        public GameplayEffectDefinition Definition { get; }

        public int Count { get; }

        public CharacterEffectState(GameplayEffectDefinition definition, int count) {
            Definition = definition;
            Count = count;
        }
    }

    public sealed class CharacterEffects : NetworkBehaviour, IComponent {

        private IGameplayEffectController[] _controllers;

        private readonly Dictionary<Type, IGameplayEffectController> _dataTypeToController = new();

        private readonly Dictionary<int, CharacterEffectState> _idToEffectState = new();

        public int Count => _idToEffectState.Count;

        public Dictionary<int, CharacterEffectState>.ValueCollection Collection => _idToEffectState.Values;

        public Dictionary<int, CharacterEffectState>.ValueCollection.Enumerator GetEnumerator() {
            return _idToEffectState.Values.GetEnumerator();
        }

        [Inject]
        internal void InjectControllers(IReadOnlyList<IGameplayEffectController> controllers) {
            _controllers = (IGameplayEffectController[])controllers;
            foreach (var controller in _controllers) {
                var dataType = controller.DataType;
                if (_dataTypeToController.TryAdd(dataType, controller)) {
                    continue;
                }
                throw new InvalidOperationException($"A controller for {dataType.FullName} is already registered.");
            }
        }

        public override void OnNetworkDespawn() {
            base.OnNetworkDespawn();
            Clear();
        }

        private void Clear() {
            foreach (var controller in _controllers) {
                controller.Clear();
            }
            _idToEffectState.Clear();
        }

        public void AddEffect(GameplayEffectDefinition definition, int count = 1) {
            ValidateDefinitionAndPositiveCount(definition, count);
            var currentCount = GetEffectCount(definition.Id);
            ApplyEffectCount(definition, currentCount + count);
        }

        public void RemoveEffect(int definitionId, int count = 1) {
            if (count <= 0) {
                throw new ArgumentOutOfRangeException(nameof(count), count, "Effect count must be greater than zero.");
            }
            if (!_idToEffectState.TryGetValue(definitionId, out var state)) {
                return;
            }
            ApplyEffectCount(state.Definition, Math.Max(0, state.Count - count));
        }

        public int GetEffectCount(int definitionId) {
            return _idToEffectState.TryGetValue(definitionId, out var state) ? state.Count : 0;
        }

        public bool TryGetEffect(int definitionId, out CharacterEffectState state) {
            return _idToEffectState.TryGetValue(definitionId, out state);
        }

        [Rpc(SendTo.Everyone)]
        public void AddEffectRpc(int definitionId, int count = 1) {
            AddEffect(GameplayEffectDefinitionRegistry.Shared[definitionId], count);
        }

        [Rpc(SendTo.Everyone)]
        public void RemoveEffectRpc(int definitionId, int count = 1) {
            RemoveEffect(definitionId, count);
        }

        protected override void OnSynchronize<T>(ref BufferSerializer<T> serializer) {
            base.OnSynchronize(ref serializer);
            if (serializer.IsWriter) {
                var writer = serializer.GetFastBufferWriter();
                writer.WriteValueSafe(Count);
                foreach (var state in _idToEffectState.Values) {
                    writer.WriteValueSafe(state.Definition.Id);
                    writer.WriteValueSafe(state.Count);
                }
            } else {
                Clear();
                var reader = serializer.GetFastBufferReader();
                reader.ReadValueSafe(out int effectCount);
                for (var i = 0; i < effectCount; i++) {
                    reader.ReadValueSafe(out int definitionId);
                    reader.ReadValueSafe(out int count);
                    ApplyEffectCount(GameplayEffectDefinitionRegistry.Shared[definitionId], count);
                }
            }
        }

        private void ApplyEffectCount(GameplayEffectDefinition definition, int count) {
            if (_idToEffectState.TryGetValue(definition.Id, out var state) && state.Count == count) {
                return;
            }

            if (count == 0) {
                if (_idToEffectState.Remove(definition.Id)) {
                    SetEffectDataCount(definition, 0);
                }
                return;
            }

            _idToEffectState[definition.Id] = new CharacterEffectState(definition, count);
            SetEffectDataCount(definition, count);
        }

        private void SetEffectDataCount(GameplayEffectDefinition definition, int count) {
            foreach (var data in definition.DataSpan) {
                if (data is not null && _dataTypeToController.TryGetValue(data.GetType(), out var controller)) {
                    controller.SetEffectCount(definition.Id, data, count);
                }
            }
        }

        private static void ValidateDefinitionAndPositiveCount(GameplayEffectDefinition definition, int count) {
            if (definition == null) {
                throw new ArgumentNullException(nameof(definition));
            }
            if (definition.Id == 0) {
                throw new InvalidOperationException($"{definition.name} is not registered.");
            }
            if (count <= 0) {
                throw new ArgumentOutOfRangeException(nameof(count), count, "Effect count must be greater than zero.");
            }
        }
    }
}
