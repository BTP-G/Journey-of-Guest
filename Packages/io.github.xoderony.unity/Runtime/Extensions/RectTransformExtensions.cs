using UnityEngine;

namespace Xoderony.Extensions {

    public static class RectTransformExtensions {

        public static void SetPositionClampInParent(this RectTransform rectTransform, Vector2 screenPoint, float horizontalPadding = 0f, float verticalPadding = 0f) {
            var parent = rectTransform.parent as RectTransform;
            var parentRect = parent.rect;
            var screenSize = new Vector2(Screen.width, Screen.height);
            var anchoredPosition = Vector2.Scale(screenPoint / screenSize, parentRect.size);
            var width = parentRect.width - (2 * horizontalPadding);
            var height = parentRect.height - (2 * verticalPadding);

            // 1. 计算当前 UI 在父容器坐标系下的四个边界
            var childRect = rectTransform.rect;
            var xMin = childRect.xMin + anchoredPosition.x;
            var xMax = childRect.xMax + anchoredPosition.x;
            var yMin = childRect.yMin + anchoredPosition.y;
            var yMax = childRect.yMax + anchoredPosition.y;
            if (xMin < 0) {
                anchoredPosition.x -= xMin;
            }
            if (xMax > width) {
                anchoredPosition.x -= xMax - width;
            }
            if (yMin < 0) {
                anchoredPosition.y -= yMin;
            }
            if (yMax > height) {
                anchoredPosition.y -= yMax - height;
            }
            rectTransform.anchoredPosition = anchoredPosition;
        }

        /// <summary>根据屏幕坐标设置 RectTransform 的位置。 使用简化的归一化计算方式，将屏幕像素坐标直接映射到父容器的相对尺寸。</summary>
        /// <param name="screenPoint">屏幕坐标（像素），例如 Input.mousePosition。</param>
        public static void SetPositionFromScreenPoint(this RectTransform rectTransform, in Vector2 screenPoint) {
            var parentRect = rectTransform.parent as RectTransform;
            var screenSize = new Vector2(Screen.width, Screen.height);
            var anchoredPosition = Vector2.Scale(screenPoint / screenSize, parentRect.rect.size);
            rectTransform.anchoredPosition = anchoredPosition;
        }

        public static void ClampInParent(this RectTransform rectTransform, float horizontalPadding = 0f, float verticalPadding = 0f) {
            var parent = rectTransform.parent as RectTransform;
            var parentSize = parent.rect.size;
            parentSize.x -= 2 * horizontalPadding;
            parentSize.y -= 2 * verticalPadding;

            // 1. 计算当前 UI 在父容器坐标系下的四个边界
            var childRect = rectTransform.rect;
            var anchoredPosition = rectTransform.anchoredPosition;
            var xMin = childRect.xMin + anchoredPosition.x;
            var xMax = childRect.xMax + anchoredPosition.x;
            var yMin = childRect.yMin + anchoredPosition.y;
            var yMax = childRect.yMax + anchoredPosition.y;

            var needsClamp = false;
            // 2. 水平方向修正左边界越界
            if (xMin < 0) {
                anchoredPosition.x -= xMin;
                needsClamp = true;
            }

            // 右边界越界
            if (xMax > parentSize.x) {
                anchoredPosition.x -= xMax - parentSize.x;
                needsClamp = true;
            }

            // 3. 垂直方向修正下边界越界
            if (yMin < 0) {
                anchoredPosition.y -= yMin;
                needsClamp = true;
            }

            if (yMax > parentSize.y) { // 上边界越界
                anchoredPosition.y -= yMax - parentSize.y;
                needsClamp = true;
            }
            // 4. 应用最终位置
            if (needsClamp) {
                rectTransform.anchoredPosition = anchoredPosition;
            }
        }
    }
}
