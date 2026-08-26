using Expriverse.Health;
using System;
using UnityEngine;
using Xoderony.GameplayEffects;
using Xoderony.Numerics;
using Xoderony.YooAsset;

namespace Expriverse.GameplayEffects.Data {

    [Serializable]
    public sealed class DamageDealtPeriodicDamageData : GameplayEffectData {

        [SerializeField]
        private YooAssetReference<PeriodicHealthChangeDefinition> _definition;

        [SerializeField, Min(1)]
        private int _tickCountPerStack = 1;

        [SerializeField, Min(0)]
        private int _tickDamage;

        [SerializeField]
        private Q16 _actualDamageMultiplier = new(1, 10);

        [SerializeField]
        private HealthChangeFlag _requiredFlags = HealthChangeFlag.Direct;

        [SerializeField]
        private HealthChangeFlag _excludedFlags = HealthChangeFlag.DamageOverTime;

        public ref YooAssetReference<PeriodicHealthChangeDefinition> Definition => ref _definition;

        public int TickCountPerStack => _tickCountPerStack;

        public int TickDamage => _tickDamage;

        public Q16 ActualDamageMultiplier => _actualDamageMultiplier;

        public HealthChangeFlag RequiredFlags => _requiredFlags;

        public HealthChangeFlag ExcludedFlags => _excludedFlags;

        protected override void OnValidate() {
            base.OnValidate();
            _tickCountPerStack = Mathf.Max(1, _tickCountPerStack);
            _tickDamage = Mathf.Max(0, _tickDamage);
            _actualDamageMultiplier = _actualDamageMultiplier < Q16.Zero ? Q16.Zero : _actualDamageMultiplier;
        }
    }
}
