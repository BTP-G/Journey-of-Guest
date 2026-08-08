using JoG.Health;
using System;
using UnityEngine;
using Xoderony.GameplayEffects;
using Xoderony.Numerics;
using Xoderony.YooAsset;

namespace JoG.GameplayEffects.Data {

    [Serializable]
    public sealed class ConditionalAreaDamageEffectData : GameplayEffectData {

        [SerializeField]
        private YooAssetReference<GameplayEffectDefinition> _requiredEffect;

        [SerializeField]
        private YooAssetReference<GameObject> _effectPrefab;

        [SerializeField, Min(0f)]
        private float _radius = 1f;

        [SerializeField]
        private LayerMask _hitLayer;

        [SerializeField, Min(0)]
        private int _damage;

        [SerializeField]
        private Q16 _actualDamageMultiplier = Q16.One;

        [SerializeField]
        private HealthChangeFlag _outputFlags = HealthChangeFlag.HolySword;

        [SerializeField]
        private HealthChangeFlag _requiredFlags = HealthChangeFlag.Direct;

        [SerializeField]
        private HealthChangeFlag _excludedFlags = HealthChangeFlag.HolySword;

        public ref YooAssetReference<GameplayEffectDefinition> RequiredEffect => ref _requiredEffect;

        public ref YooAssetReference<GameObject> EffectPrefab => ref _effectPrefab;

        public float Radius => _radius;

        public LayerMask HitLayer => _hitLayer;

        public int Damage => _damage;

        public Q16 ActualDamageMultiplier => _actualDamageMultiplier;

        public HealthChangeFlag OutputFlags => _outputFlags;

        public HealthChangeFlag RequiredFlags => _requiredFlags;

        public HealthChangeFlag ExcludedFlags => _excludedFlags;

        protected override void OnValidate() {
            base.OnValidate();
            _radius = Mathf.Max(0f, _radius);
            _damage = Mathf.Max(0, _damage);
            _actualDamageMultiplier = _actualDamageMultiplier < Q16.Zero ? Q16.Zero : _actualDamageMultiplier;
        }
    }
}
