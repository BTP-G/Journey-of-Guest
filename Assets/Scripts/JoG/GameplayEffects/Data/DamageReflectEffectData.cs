using JoG.Health;
using System;
using UnityEngine;
using Xoderony.GameplayEffects;
using Xoderony.Numerics;

namespace JoG.GameplayEffects.Data {

    [Serializable]
    public sealed class DamageReflectEffectData : GameplayEffectData {

        [SerializeField]
        private Q16 _actualDamageMultiplier = new(1, 2);

        [SerializeField]
        private HealthChangeFlag _outputFlags = HealthChangeFlag.Reflect;

        [SerializeField]
        private HealthChangeFlag _requiredFlags = HealthChangeFlag.Direct;

        [SerializeField]
        private HealthChangeFlag _excludedFlags = HealthChangeFlag.Reflect;

        public Q16 ActualDamageMultiplier => _actualDamageMultiplier;

        public HealthChangeFlag OutputFlags => _outputFlags;

        public HealthChangeFlag RequiredFlags => _requiredFlags;

        public HealthChangeFlag ExcludedFlags => _excludedFlags;

        protected override void OnValidate() {
            base.OnValidate();
            _actualDamageMultiplier = _actualDamageMultiplier < Q16.Zero ? Q16.Zero : _actualDamageMultiplier;
        }
    }
}
