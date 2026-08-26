using ANU.IngameDebug.Console;
using Cysharp.Text;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Xoderony.Logging;

[assembly: RegisterDebugCommandTypes(typeof(Expriverse.GameplayEffects.PeriodicHealthChangeDefinitionDictionary))]

namespace Expriverse.GameplayEffects {

    [DebugCommandPrefix("periodic-health-change")]
    public sealed class PeriodicHealthChangeDefinitionDictionary : IReadOnlyDictionary<int, PeriodicHealthChangeDefinition> {

        public static readonly PeriodicHealthChangeDefinitionDictionary Shared = new();

        private readonly Dictionary<int, PeriodicHealthChangeDefinition> _idToDefinition = new();

        IEnumerable<int> IReadOnlyDictionary<int, PeriodicHealthChangeDefinition>.Keys => _idToDefinition.Keys;

        IEnumerable<PeriodicHealthChangeDefinition> IReadOnlyDictionary<int, PeriodicHealthChangeDefinition>.Values => _idToDefinition.Values;

        public int Count => _idToDefinition.Count;

        public Dictionary<int, PeriodicHealthChangeDefinition>.KeyCollection Keys => _idToDefinition.Keys;

        public Dictionary<int, PeriodicHealthChangeDefinition>.ValueCollection Definitions => _idToDefinition.Values;

        public PeriodicHealthChangeDefinition this[int id] => _idToDefinition[id];

        [DebugCommand]
        public static void PrintDefinitions() {
            using var builder = ZString.CreateStringBuilder(true);
            foreach (var definition in Shared.Definitions) {
                builder.Append("id: ");
                builder.Append(definition.Id);
                builder.Append("; definition name: ");
                builder.AppendLine(definition.name);
            }
            Shared.Log(builder.ToString());
        }

        public void Add(PeriodicHealthChangeDefinition definition) {
            if (definition.Id != 0) {
                this.LogError("已经注册过此对象！");
                return;
            }

            var id = Animator.StringToHash(definition.name);
            if (id == 0) {
                throw new InvalidOperationException($"{definition.name} produced the reserved periodic health change ID 0.");
            }
            if (_idToDefinition.TryGetValue(id, out var existingDefinition)) {
                throw new InvalidOperationException($"Periodic health change ID collision: [{definition.name}={id}], [{existingDefinition.name}={existingDefinition.Id}].");
            }
            definition.Id = id;
            _idToDefinition.Add(id, definition);
        }

        public bool Remove(PeriodicHealthChangeDefinition definition) {
            if (!_idToDefinition.Remove(definition.Id)) {
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

        public bool TryGetValue(int id, out PeriodicHealthChangeDefinition definition) {
            return _idToDefinition.TryGetValue(id, out definition);
        }

        IEnumerator<KeyValuePair<int, PeriodicHealthChangeDefinition>> IEnumerable<KeyValuePair<int, PeriodicHealthChangeDefinition>>.GetEnumerator() {
            return _idToDefinition.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return _idToDefinition.GetEnumerator();
        }
    }
}
