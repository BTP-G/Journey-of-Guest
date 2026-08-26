using EditorAttributes;
using Expriverse.Character;
using Expriverse.Interaction;
using Expriverse.UI;
using System;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using Xoderony.GameplayEffects;
using Xoderony.Localization;
using Xoderony.YooAsset;

namespace Expriverse.Props {

    public class DemonAltarInteraction : NetworkBehaviour, IInteractable, IWorldTooltipSource {
        public InteractionEvent onInteracted = new();
        public YooAssetReference<GameplayEffectDefinition> effectDefinition;
        public int effectCount = 1;
        [Required] public Transform tooltipPoint;

        [LocalizationKey(@"^interact\..*\.name$")]
        public string nameKey;

        [LocalizationKey(@"^interact\..*\.desc$")]
        public string descKey;

        [Range(0, 100)] public int healthCostPercentage;
        private GameplayEffectDefinition _effect;
        Vector3 IWorldTooltipSource.TooltipPosition => tooltipPoint.position;

        public void Awake() {
            effectDefinition.Load();
            _effect = effectDefinition.AssetObject;
        }

        public override void OnDestroy() {
            base.OnDestroy();
            effectDefinition.Unload();
        }

        void ITooltipSource.BuildTooltip(StringBuilder builder) {
            //builder.AppendLine(Localizer.GetString(nameKey))
            //    .Append(Localizer.GetString(descKey, healthCostPercentage, effectCount, _effect));
        }

        bool IInteractable.CanInteract(Entity interactor) {
            return interactor.HasAuthority
                && interactor is CharacterEntity entity
                && entity.Health.Ratio * 100 > healthCostPercentage;
        }

        void IInteractable.OnInteracted(Entity interactor) {
            //var entity = interactor as CharacterEntity;
            //entity.Health.networkHealth.Value -= Mathf.CeilToInt(entity.Health.MaxHealth * healthCostPercentage / 100f);
            //entity.Effects.AddEffectRpc(_effect.Id, effectCount);
            InvokeOnInteractedRpc(interactor);
        }

        [Rpc(SendTo.Everyone)]
        private void InvokeOnInteractedRpc(Entity interactor) {
            onInteracted.Invoke(interactor);
        }

        [Serializable]
        public class InteractionEvent : UnityEvent2<Entity> { }
    }
}
