using EditorAttributes;
using Expriverse.UI;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using Xoderony.GameplayEffects;
using Xoderony.Localization;
using Xoderony.Logging;

namespace Expriverse.Item {

    [CreateAssetMenu(fileName = "ItemData", menuName = "Expriverse/Item Data")]
    public class ItemData : GameplayEffectDefinition, ITooltipSource {

        [LocalizationKey(@"^item\..*\.name$")]
        public string nameKey;

        [LocalizationKey(@"^item\..*\.desc$")]
        public string descriptionKey;

        [Required]
        public NetworkObject pickupPrefab;

        [Required]
        [AssetPreview(100, 100)]
        public Sprite iconSprite;

        public virtual string ItemName => Localizer.GetString(nameKey);

        public virtual string Description => Localizer.GetString(descriptionKey);

        public void BuildTooltip(StringBuilder builder) {
            builder.Append(Localizer.GetString(nameKey))
                   .AppendLine()
                   .AppendLine(Localizer.GetString(descriptionKey));
        }

        protected override void OnValidate() {
            base.OnValidate();
            if ((pickupPrefab != null) && (pickupPrefab.GetComponentInChildren<ItemPickupBehaviour>() == null)) {
                this.LogWarning($"[{nameof(pickupPrefab)}: {pickupPrefab}] 缺少{nameof(ItemPickupBehaviour)}组件。");
                pickupPrefab = null;
            }
        }
    }
}

