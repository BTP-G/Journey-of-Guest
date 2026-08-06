using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Xoderony.GameplayEffects {

    public sealed class GameplayEffectDefinitionRegistry : IReadOnlyDictionary<int, GameplayEffectDefinition> {

        public static readonly GameplayEffectDefinitionRegistry Shared = new();

        private readonly Dictionary<int, GameplayEffectDefinition> _idToDefinition = new();

        IEnumerable<int> IReadOnlyDictionary<int, GameplayEffectDefinition>.Keys => _idToDefinition.Keys;

        IEnumerable<GameplayEffectDefinition> IReadOnlyDictionary<int, GameplayEffectDefinition>.Values => _idToDefinition.Values;

        public int Count => _idToDefinition.Count;

        public Dictionary<int, GameplayEffectDefinition>.KeyCollection Keys => _idToDefinition.Keys;

        public Dictionary<int, GameplayEffectDefinition>.ValueCollection Definitions => _idToDefinition.Values;

        public GameplayEffectDefinition this[int id] => _idToDefinition[id];

        public void Add(GameplayEffectDefinition definition) {
            if (definition == null) {
                throw new ArgumentNullException(nameof(definition));
            }
            if (definition.ReadOnly) {
                throw new InvalidOperationException($"{definition.name} is already registered.");
            }

            var id = Animator.StringToHash(definition.name);
            if (id == 0) {
                throw new InvalidOperationException($"{definition.name} produced the reserved gameplay effect ID 0.");
            }
            if (_idToDefinition.TryGetValue(id, out var existingDefinition)) {
                throw new InvalidOperationException($"Gameplay effect ID collision: [{definition.name}={id}], [{existingDefinition.name}={existingDefinition.Id}].");
            }
            definition.Id = id;
            _idToDefinition.Add(id, definition);
        }

        public bool Remove(GameplayEffectDefinition definition) {
            if (definition == null || !_idToDefinition.Remove(definition.Id)) {
                return false;
            }
            definition.Id = default;
            return true;
        }

        public void Clear() {
            foreach (var definition in _idToDefinition.Values) {
                definition.Id = default;
            }
            _idToDefinition.Clear();
        }

        public bool ContainsKey(int id) {
            return _idToDefinition.ContainsKey(id);
        }

        public bool TryGetValue(int id, out GameplayEffectDefinition definition) {
            return _idToDefinition.TryGetValue(id, out definition);
        }

        IEnumerator<KeyValuePair<int, GameplayEffectDefinition>> IEnumerable<KeyValuePair<int, GameplayEffectDefinition>>.GetEnumerator() {
            return _idToDefinition.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return _idToDefinition.GetEnumerator();
        }
    }
}
