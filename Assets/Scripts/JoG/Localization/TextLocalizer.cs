using EditorAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using Xoderony.Localization;

namespace JoG.Localization {

    public sealed class TextLocalizer : MonoBehaviour {

        [Required]
        [FormerlySerializedAs("tmp_text")]
        public TMP_Text text;

        [LocalizationKey]
        [FormerlySerializedAs("key")]
        public string textKey;

        public void UpdateText() {
            text.text = Localizer.GetString(textKey);
        }

        private void Awake() {
            Localizer.OnLanguageUpdated += UpdateText;
        }

        private void OnDestroy() {
            Localizer.OnLanguageUpdated -= UpdateText;
        }

        private void Reset() {
            text = GetComponentInChildren<TMP_Text>();
        }

        private void OnValidate() {
            if (Application.isPlaying) {
                return;
            }

            if (text == null) {
                text = GetComponentInChildren<TMP_Text>(true);
                if (text == null) {
                    return;
                }
            }
            text.text = textKey;
        }
    }
}
