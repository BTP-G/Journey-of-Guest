using EditorAttributes;
using Xoderony.Localization;
using Xoderony.Logging;
using JoG.Buff;
using JoG.UI;
using System;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace JoG.Item {

    [CreateAssetMenu(fileName = "ItemData", menuName = "JoG/Item Data")]
    public class ItemData : ScriptableObject, ITooltipSource {

        [LocalizationKey(@"^item\..*\.name$")]
        public string nameKey;

        [LocalizationKey(@"^item\..*\.desc$")]
        public string descriptionKey;

        [Required]
        public NetworkObject pickupPrefab;

        [Required]
        [AssetPreview(100, 100)]
        public Sprite iconSprite;

        public BuffDefinition[] buffDatas = Array.Empty<BuffDefinition>();

        public virtual string ItemName => Localizer.GetString(nameKey);

        public virtual string Description => Localizer.GetString(descriptionKey);

        public void BuildTooltip(StringBuilder builder) {
            builder.Append(Localizer.GetString(nameKey))
                   .AppendLine()
                   .AppendLine(Localizer.GetString(descriptionKey));
        }

        protected virtual void OnValidate() {
            if ((pickupPrefab != null) && (pickupPrefab.GetComponentInChildren<ItemPickupBehaviour>() == null)) {
                this.LogWarning($"[{nameof(pickupPrefab)}: {pickupPrefab}] 缺少{nameof(ItemPickupBehaviour)}组件。");
                pickupPrefab = null;
            }
        }

    }

}
