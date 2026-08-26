using EditorAttributes;
using System;
using UnityEngine;

namespace Expriverse.UI {

    [Serializable]
    public class Crosshair {
        public const float cofficient = 1870f;
        [SerializeField, Required] private RectTransform crosshairTransform;

        public void SetSpread(Vector2 spread) {
            var x = cofficient * Mathf.Tan(spread.x * Mathf.Deg2Rad);
            var y = cofficient * Mathf.Tan(spread.y * Mathf.Deg2Rad);
            crosshairTransform.sizeDelta = new Vector2(x, y);
        }
    }
}
