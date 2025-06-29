using EditorAttributes;
using JoG.Character;
using JoG.Magic;
using UnityEngine;

namespace JoG.UI {

    public class Crosshair : MonoBehaviour {
        public const float cofficient = 1870f;
        [SerializeField, Required] private RectTransform crosshairTransform;

        [Button]
        public void SetSpread(Vector2 spread) {
            var x = cofficient * Mathf.Tan(spread.x * Mathf.Deg2Rad);
            var y = cofficient * Mathf.Tan(spread.y * Mathf.Deg2Rad);
            crosshairTransform.sizeDelta = new Vector2(x, y);
        }
    }
}