using System;
using System.Collections.Generic;
using UnityEngine;

namespace JoG.Gameplay {

    [Serializable]
    public class RarityWeight {
        public EffectRarity rarity;
        public int weight = 1;
    }

    [CreateAssetMenu(fileName = "AltarEffectPool", menuName = "JoG/Difficulty/Altar Effect Pool")]
    public class AltarEffectPool : ScriptableObject {

        [Header("Rarity Weights")]
        public RarityWeight[] rarityWeights = new RarityWeight[] {
            new RarityWeight { rarity = EffectRarity.Common, weight = 60 },
            new RarityWeight { rarity = EffectRarity.Rare, weight = 30 },
            new RarityWeight { rarity = EffectRarity.Legend, weight = 10 }
        };

        [Header("Bless Effects")]
        public AltarEffectData[] blessEffects = Array.Empty<AltarEffectData>();

        [Header("Demon Effects")]
        public AltarEffectData[] demonEffects = Array.Empty<AltarEffectData>();

        [Header("Debug")]
        [SerializeField] private bool useWeightedRandom = true;

        private Dictionary<EffectRarity, List<AltarEffectData>> _blessCache;
        private Dictionary<EffectRarity, List<AltarEffectData>> _demonCache;
        private int _totalWeight;

        public void Initialize() {
            _blessCache = new Dictionary<EffectRarity, List<AltarEffectData>>();
            _demonCache = new Dictionary<EffectRarity, List<AltarEffectData>>();

            foreach (EffectRarity rarity in Enum.GetValues(typeof(EffectRarity))) {
                _blessCache[rarity] = new List<AltarEffectData>();
                _demonCache[rarity] = new List<AltarEffectData>();
            }

            foreach (var effect in blessEffects) {
                if (effect.rarity == EffectRarity.Common) _blessCache[EffectRarity.Common].Add(effect);
                else if (effect.rarity == EffectRarity.Rare) _blessCache[EffectRarity.Rare].Add(effect);
                else if (effect.rarity == EffectRarity.Legend) _blessCache[EffectRarity.Legend].Add(effect);
            }

            foreach (var effect in demonEffects) {
                if (effect.rarity == EffectRarity.Common) _demonCache[EffectRarity.Common].Add(effect);
                else if (effect.rarity == EffectRarity.Rare) _demonCache[EffectRarity.Rare].Add(effect);
                else if (effect.rarity == EffectRarity.Legend) _demonCache[EffectRarity.Legend].Add(effect);
            }

            _totalWeight = 0;
            foreach (var rw in rarityWeights) {
                _totalWeight += rw.weight;
            }
        }

        public AltarEffectData GetRandomEffect(AltarType altarType) {
            if (_blessCache == null) Initialize();

            var cache = altarType == AltarType.Bless ? _blessCache : _demonCache;

            if (useWeightedRandom) {
                return GetWeightedRandomEffect(cache, altarType);
            } else {
                return GetUniformRandomEffect(cache, altarType);
            }
        }

        private AltarEffectData GetWeightedRandomEffect(Dictionary<EffectRarity, List<AltarEffectData>> cache, AltarType altarType) {
            int randomValue = UnityEngine.Random.Range(0, _totalWeight);
            int currentWeight = 0;
            EffectRarity selectedRarity = EffectRarity.Common;

            foreach (var rw in rarityWeights) {
                currentWeight += rw.weight;
                if (randomValue < currentWeight) {
                    selectedRarity = rw.rarity;
                    break;
                }
            }

            var effects = cache[selectedRarity];
            if (effects.Count == 0) {
                return GetFallbackEffect(cache);
            }

            return effects[UnityEngine.Random.Range(0, effects.Count)];
        }

        private AltarEffectData GetUniformRandomEffect(Dictionary<EffectRarity, List<AltarEffectData>> cache, AltarType altarType) {
            var allEffects = new List<AltarEffectData>();
            foreach (var kvp in cache) {
                allEffects.AddRange(kvp.Value);
            }

            if (allEffects.Count == 0) {
                return null;
            }

            return allEffects[UnityEngine.Random.Range(0, allEffects.Count)];
        }

        private AltarEffectData GetFallbackEffect(Dictionary<EffectRarity, List<AltarEffectData>> cache) {
            foreach (var kvp in cache) {
                if (kvp.Value.Count > 0) {
                    return kvp.Value[0];
                }
            }
            return null;
        }
    }
}
