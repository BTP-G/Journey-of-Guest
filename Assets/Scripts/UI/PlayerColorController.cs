using EditorAttributes;
using UnityEngine;

namespace JoG.UI {

    public class PlayerColorController : MonoBehaviour {

        [field: SerializeField, Required]
        public ColorSlider ColorSlider { get; private set; }

        public void UpdatePlayerColor(Color color) {
            var colorStr = ColorUtility.ToHtmlStringRGBA(color);
            PlayerPrefs.SetString("player_color", $"#{colorStr}");
        }

        private void Awake() {
            var colorStr = PlayerPrefs.GetString("player_color", "#FFFFFFFF");
            ColorUtility.TryParseHtmlString(colorStr, out var color);
            ColorSlider.Color = color;
            ColorSlider.OnColorChanged.AddListener(UpdatePlayerColor);
        }
    }
}