using JoG.Item;
using JoG.UI;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace JoG.Inventory {

    public partial class Slot : IPointerEnterHandler, IPointerMoveHandler, IPointerExitHandler, IPointerClickHandler {

        [SerializeField]
        [HideInInspector] internal ScreenTooltip tooltipView;

        public event Action<ItemData, int> DropRequested;

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
            DropRequested?.Invoke(_itemData, dropAmount);
        }

        private void OnDisable() {
            tooltipView?.Hide(0);
        }

    }

}

