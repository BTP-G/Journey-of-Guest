using JoG.Character;
using JoG.Item;
using JoG.UI;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace JoG.Inventory {

    public partial class Slot : IPointerEnterHandler, IPointerMoveHandler, IPointerExitHandler, IPointerClickHandler {

        [SerializeField]
        [HideInInspector] internal ScreenTooltip tooltipView;

        private NetworkInventory _inventory;

        private CharacterEntity _entity;

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData) {
            tooltipView.SetTooltip(_itemData);
            tooltipView.SetPosition(eventData.pointerCurrentRaycast.screenPosition);
            tooltipView.Show(0.2f);
        }

        void IPointerMoveHandler.OnPointerMove(PointerEventData eventData) {
            tooltipView.SetPosition(eventData.pointerCurrentRaycast.screenPosition);
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData) {
            tooltipView.Hide(0);
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData) {
            var dropAmount = eventData.button switch {
                PointerEventData.InputButton.Left => 1,
                PointerEventData.InputButton.Right => Math.Max(1, _itemCount / 2),
                _ => _itemCount,
            };
            var position = _entity.Model.Center;
            var velocity = _entity.Rigidbody.GetPointVelocity(position);
            var direction = new Vector3(
                0,
                5,
                1
            );
            velocity += _entity.Rigidbody.rotation * direction;
            var item = _inventory.networkObjectFactory
                                 .Instantiate(
                                     _itemData.pickupPrefab,
                                     position: position,
                                     rotation: Quaternion.identity
                                 );
            item.GetComponent<ItemPickupBehaviour>().Amount = dropAmount;
            item.Spawn(true);
            item.GetComponent<Rigidbody>().linearVelocity = velocity;
            _inventory.RemoveItemRpc(_itemData, dropAmount);
        }

        private void Awake() {
            _inventory = GetComponentInParent<NetworkInventory>();
            _entity = GetComponentInParent<CharacterEntity>();
        }

        private void OnDisable() {
            tooltipView.Hide(0);
        }

    }

}
