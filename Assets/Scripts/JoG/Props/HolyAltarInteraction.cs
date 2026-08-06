using EditorAttributes;
using Xoderony.Localization;
using Xoderony.YooAsset;
using JoG.Character;
using JoG.Interaction;
using JoG.Inventory;
using JoG.Item;
using JoG.Localization;
using JoG.Networking;
using JoG.UI;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using VContainer;

namespace JoG.Props {

    public class HolyAltarInteraction : NetworkBehaviour, IInteractable, IWorldTooltipSource {
        public YooAssetReference<ItemDropTable> dropTableReference;
        public YooAssetReference<ItemData> itemRequired;

        [LocalizationKey(@"^interact\..*\.name$")]
        public string nameKey;

        [LocalizationKey(@"^interact\..*\.desc$")]
        public string descKey;

        [Required] public Transform tooltipPoint;
        [Inject] internal NetworkObjectFactory networkObjectFactory;

        public Vector3 TooltipPosition => tooltipPoint.position;

        public void BuildTooltip(StringBuilder builder) {
            builder.AppendLine(Localizer.GetString(nameKey))
                .Append(Localizer.GetString(descKey));
        }

        public bool CanInteract(Entity interactor) {
            return interactor.HasAuthority
              && interactor.TryGetComponent(out CharacterInventory inventory, null)
              && inventory.GetItemCount(itemRequired.AssetObject) > 0;
        }

        public void OnInteracted(Entity interactor) {
            var inventory = interactor.GetComponent<CharacterInventory>(null);
            if (!inventory.RemoveItem(itemRequired.AssetObject, 1)) {
                return;
            }
            var dropItem = dropTableReference.AssetObject.Get();
            var item = networkObjectFactory.Instantiate(
                 dropItem,
                 position: tooltipPoint.position,
                 rotation: Quaternion.identity);
            item.GetComponent<ItemPickupBehaviour>().Amount = 1;
            item.Spawn(true);
            item.GetComponent<Rigidbody>().linearVelocity = -Physics.gravity;
        }

        private void Awake() {
            dropTableReference.Load();
            itemRequired.Load();
        }
    }
}

