using EditorAttributes;
using Expriverse.GameplayEffects.Data;
using Expriverse.Health;
using UnityEngine;
using Xoderony.GameplayEffects;

namespace Expriverse.GameplayEffects {

    [CreateAssetMenu(menuName = "Expriverse/" + nameof(PeriodicHealthChangeDefinition), fileName = nameof(PeriodicHealthChangeDefinition))]
    public sealed class PeriodicHealthChangeDefinition : ScriptableObject {

        [SerializeField, Required]
        private GameplayEffectDefinition _displayEffectDefinition;

        [SerializeField, Min(0.02f)]
        private float _tickInterval;

        [SerializeField]
        private MergeMode _tickCountMergeMode;

        [SerializeField]
        private MergeMode _tickValueMergeMode;

        [SerializeField]
        private HealthChangeFlag _healthChangeFlags;

        [SerializeField]
        private Color32 _color;

        public int Id { get; internal set; }

        public GameplayEffectDefinition DisplayEffectDefinition => _displayEffectDefinition;

        public float TickInterval => _tickInterval;

        public MergeMode TickCountMergeMode => _tickCountMergeMode;

        public MergeMode TickValueMergeMode => _tickValueMergeMode;

        public HealthChangeFlag HealthChangeFlags => _healthChangeFlags;

        public Color32 Color => _color;

        private void OnValidate() {
            _tickInterval = Mathf.Max(0.02f, _tickInterval);
        }
    }
}
