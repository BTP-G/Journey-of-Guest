using TMPro;
using UnityEngine;

namespace JoG.UI {

    public class ToggleLabelColorController : MonoBehaviour {
        public Color normalColor;
        public Color selectedColor;
        [SerializeField] private TMP_Text label;

        public void UpdateLabelColor(bool isOn) {
            if (isOn) {
                label.color = selectedColor;
            } else {
                label.color = normalColor;
            }
        }

        private void Awake() {
            label.color = normalColor;
        }
    }
}