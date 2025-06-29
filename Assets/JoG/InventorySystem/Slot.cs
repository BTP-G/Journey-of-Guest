using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace JoG.InventorySystem {

    public class Slot : MonoBehaviour, IPointerEnterHandler, IPointerMoveHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler {
        [NonSerialized] public InventoryUIController controller;
        [NonSerialized] public int index;
        public GameObject iconGameObject;
        public Image iconImage;
        public TMP_Text countText;

        public void UpdateView(InventoryItem inventoryItem) {
            if (inventoryItem is null || inventoryItem.Count <= 0) {
                iconGameObject.SetActive(false);
                return;
            }
            iconGameObject.SetActive(true);
            iconImage.sprite = inventoryItem.Icon;
            countText.text = inventoryItem.Count > 1 ? inventoryItem.Count.ToString() : string.Empty;
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData) {
        }

        void IPointerMoveHandler.OnPointerMove(PointerEventData eventData) {
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData) {
        }

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData) {
            if (iconGameObject.activeSelf) {
                iconGameObject.SetActive(false);
                controller.ShowDragItem(iconImage.sprite, countText.text);
            }
        }

        void IDragHandler.OnDrag(PointerEventData eventData) {
            if (eventData.dragging) {
                controller.SetDragItemPosition(eventData.pointerCurrentRaycast.worldPosition);
            }
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData) {
            if (eventData.pointerEnter is not null && eventData.pointerEnter.CompareTag("Slot")) {
                var otherSlot = eventData.pointerEnter.GetComponent<Slot>();
                controller.ExchangeItem(index, otherSlot.index);
            } else {
                iconGameObject.SetActive(true);
            }
            controller.HideDragItem();
        }
    }
}