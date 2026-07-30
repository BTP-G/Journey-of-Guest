using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

namespace JoG.UI {

    public abstract class TooltipTrigger : MonoBehaviour, ITooltipSource, IPointerEnterHandler, IPointerMoveHandler, IPointerExitHandler {
        public ScreenTooltip screenTooltip;
        public float fadeIn = 0.1f;
        public float fadeOut = 0.1f;

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData) {
            screenTooltip.Show(fadeIn);
            screenTooltip.SetTooltip(this);
        }

        void IPointerMoveHandler.OnPointerMove(PointerEventData eventData) {
            screenTooltip.SetPosition(eventData.position);
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData) {
            screenTooltip.Hide(fadeOut);
        }

        public abstract void BuildTooltip(StringBuilder builder);

        protected virtual void Reset() {
            screenTooltip = FindFirstObjectByType<ScreenTooltip>();
        }
    }
}
