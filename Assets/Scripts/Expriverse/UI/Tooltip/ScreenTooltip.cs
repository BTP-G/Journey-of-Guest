using UnityEngine;

namespace Expriverse.UI {

    [RequireComponent(typeof(CanvasGroup))]
    public sealed class ScreenTooltip : TextTooltip {
        private RectTransform _parentTransform;

        /// <summary>把 Tooltip 指定锚点贴到屏幕坐标</summary>
        /// <param name="screenPoint">屏幕像素坐标</param>
        public void SetPosition(Vector2 screenPoint) {
            var parentRect = _parentTransform.rect;
            var rect = rectTransform.rect;
            var screenSize = new Vector2(Screen.width, Screen.height);
            var anchoredPosition = Vector2.Scale(screenPoint / screenSize, parentRect.size);
            var leftBottomX = rect.xMin + anchoredPosition.x;
            var leftBottomY = rect.yMin + anchoredPosition.y;
            var rightTopX = rect.xMax + anchoredPosition.x;
            var rightTopY = rect.yMax + anchoredPosition.y;
            var width = parentRect.width;
            var height = parentRect.height;
            if (leftBottomX < 0) {
                anchoredPosition.x -= leftBottomX;
            }

            if (leftBottomY < 0) {
                anchoredPosition.y -= leftBottomY;
            }

            if (rightTopX > width) {
                anchoredPosition.x += width - rightTopX;
            }

            if (rightTopY > height) {
                anchoredPosition.y += height - rightTopY;
            }

            rectTransform.anchoredPosition = anchoredPosition;
        }

        protected override void Awake() {
            base.Awake();
            _parentTransform = rectTransform.parent as RectTransform;
        }
    }
}
