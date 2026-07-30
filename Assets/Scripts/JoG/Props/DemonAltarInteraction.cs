using EditorAttributes;
using Xoderony.Localization;
using Xoderony.YooAsset;
using JoG.Buff;
using JoG.Character;
using JoG.Interaction;
using JoG.UI;
using System;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace JoG.Props {

    public class DemonAltarInteraction : NetworkBehaviour, IInteractable, IWorldTooltipSource {
        public InteractionEvent onInteracted = new();
        public YooAssetReference<GameObject> buffPrefab;
        public int buffCount = 1;
        [Required] public Transform tooltipPoint;

        [LocalizationKey(@"^interact\..*\.name$")]
        public string nameKey;

        [LocalizationKey(@"^interact\..*\.desc$")]
        public string descKey;

        [Range(0, 100)] public int healthCostPercentage;
        private BuffDefinition _buffData;
        Vector3 IWorldTooltipSource.TooltipPosition => tooltipPoint.position;

        public void Awake() {
            buffPrefab.Load();
            _buffData = buffPrefab.AssetObject.GetComponent<BuffDefinition>();
        }

        public override void OnDestroy() {
            base.OnDestroy();
            buffPrefab.Unload();
        }

        void ITooltipSource.BuildTooltip(StringBuilder builder) {
            //builder.AppendLine(Localizer.GetString(nameKey))
            //    .Append(Localizer.GetString(descKey, healthCostPercentage, buffCount, _buffData));
        }

        bool IInteractable.CanInteract(Entity interactor) {
            return interactor.HasAuthority
                && interactor is CharacterEntity entity
                && entity.Health.Ratio * 100 > healthCostPercentage;
        }

        void IInteractable.OnInteracted(Entity interactor) {
            //var buff = _buffData.Get();
            //foreach (var component in buff.ComponentSpan) {
            //    if (component is Counter counter) {
            //        counter.count = buffCount;
            //    }
            //}
            //var entity = interactor as CharacterEntity;
            //entity.Health.networkHealth.Value -= Mathf.CeilToInt(entity.Health.MaxHealth * healthCostPercentage / 100f);
            //entity.Buffs.AddBuffRpc(buff);
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
