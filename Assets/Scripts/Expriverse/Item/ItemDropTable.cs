using EditorAttributes;
using System;
using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEngine;
using Xoderony.Extensions;
using Random = UnityEngine.Random;

namespace Expriverse.Item {

    [CreateAssetMenu(fileName = "NewItemDropTable", menuName = "Expriverse/Item Drop Table")]
    public class ItemDropTable : ScriptableObject {

        [ReadOnly]
        [SerializeField]
        private int _totalWeight;

        [SerializeField]
        private Entry[] _entries = Array.Empty<Entry>();

        public int TotalWeight => _totalWeight;

        public int Count => _entries.Length;

        public Entry[] Entries {
            get => _entries;
            set {
                _entries = value ?? Array.Empty<Entry>();
                RecalculateTotalWeight();
            }
        }

        public NetworkObject Get() {
            var randomWeight = Random.Range(0, _totalWeight + 1);
            var currentWeight = 0;
            foreach (ref readonly var entry in _entries.AsReadOnlySpan()) {
                currentWeight += entry.weight;
                if (randomWeight <= currentWeight) {
                    return entry.items
                                .GetRandomElement();
                }
            }
            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RecalculateTotalWeight() {
            _totalWeight = 0;
            foreach (ref var entry in _entries.AsSpan()) {
                _totalWeight += entry.weight;
            }
        }

        [Serializable]
        public struct Entry {

            [Min(1)]
            public int weight;

            public NetworkObject[] items;

        }
    }
}
