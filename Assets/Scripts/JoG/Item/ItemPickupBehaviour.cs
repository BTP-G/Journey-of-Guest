using EditorAttributes;
using Xoderony.Extensions;
using Xoderony.Logging;
using Xoderony.YooAsset;
using JoG.Interaction;
using JoG.Inventory;
using JoG.UI;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace JoG.Item {

    [RequireComponent(typeof(Collider))]
    public class ItemPickupBehaviour : NetworkBehaviour, IInteractable, IWorldTooltipSource {
        public YooAssetReference<ItemData> itemDataReference;
        [Required] public Transform tooltipPoint;
        private float _offsetY;
        private int _amount = 1;

        public int Amount {
            get => _amount;
            set {
                if (IsSpawned) {
                    this.LogWarning("请在道具生成之前设置数量");
                } else {
                    _amount = value;
                }
            }
        }

        public ItemData ItemData => itemDataReference.AssetObject;

        Vector3 IWorldTooltipSource.TooltipPosition => transform.position.AddY(_offsetY);

        void ITooltipSource.BuildTooltip(StringBuilder builder) {
            ItemData.BuildTooltip(builder);
        }

        public override void OnDestroy() {
            base.OnDestroy();
            itemDataReference.Unload();
        }

        bool IInteractable.CanInteract(Entity entity) {
            return entity.TryGetComponent<CharacterInventoryNetwork>(out _);
        }

        void IInteractable.OnInteracted(Entity entity) {
            GivePickupRpc(entity.GetComponent<CharacterInventoryNetwork>());
        }

        protected override void OnSynchronize<T>(ref BufferSerializer<T> serializer) {
            serializer.SerializeValue(ref _amount);
        }

        private void Awake() {
            itemDataReference.Load();
            _offsetY = GetComponent<Collider>().bounds.size.y;
        }

        [Rpc(SendTo.Authority)]
        private void GivePickupRpc(NetworkBehaviourReference inventoryReference) {
            if (!IsSpawned) return;
            if (inventoryReference.TryGet<CharacterInventoryNetwork>(out var inventoryNetwork)) {
                inventoryNetwork.AddItemRpc(ItemData, Amount);
            }
            NetworkObject.Despawn();
        }
    }
}

