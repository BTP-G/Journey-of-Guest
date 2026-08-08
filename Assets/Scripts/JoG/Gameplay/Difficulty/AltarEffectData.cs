using System;
using UnityEngine;
using Xoderony.GameplayEffects;

namespace JoG.Gameplay {

    public enum AltarType {
        Bless,
        Demon
    }

    public enum EffectRarity {
        Common,
        Rare,
        Legend
    }

    [Serializable]
    public class AltarEffectData {
        public string effectId;
        public string effectName;
        public string description;
        public AltarType altarType;
        public EffectRarity rarity;
        [Range(0, 100)] public int healthCostPercentage;
        public float duration;
        public GameplayEffectDefinition effectDefinition;
        public int effectCount = 1;

        public bool HasCost => altarType == AltarType.Demon && healthCostPercentage > 0;
    }
}
