using JoG.Health;
using System;
using UnityEngine;
using Xoderony.GameplayEffects;
using Xoderony.YooAsset;

namespace JoG.GameplayEffects.Data {

    [Serializable]
    public sealed class DamageDealtEffectData : GameplayEffectData {

        [SerializeField]
        private YooAssetReference<GameplayEffectDefinition> _appliedEffect;

        [SerializeField, Min(1)]
        private int _countPerStack = 1;

        [SerializeField]
        private HealthChangeFlag _requiredFlags = HealthChangeFlag.Direct;

        [SerializeField]
        private HealthChangeFlag _excludedFlags;

        public ref YooAssetReference<GameplayEffectDefinition> AppliedEffect => ref _appliedEffect;

        public int CountPerStack => _countPerStack;

        public HealthChangeFlag RequiredFlags => _requiredFlags;

        public HealthChangeFlag ExcludedFlags => _excludedFlags;

        protected override void OnValidate() {
            base.OnValidate();
            _countPerStack = Mathf.Max(1, _countPerStack);
        }
    }
}
