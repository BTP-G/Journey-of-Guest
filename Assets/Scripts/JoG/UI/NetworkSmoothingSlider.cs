using EditorAttributes;
using TMPro;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.UI;

namespace JoG.UI {

    public class NetworkSmoothingSlider : MonoBehaviour {
        public const string Key = "jog_netcode_interpolation_offset";
        [Required] public Slider valueSlider;
        [Required] public TMP_Text valueText;

        private void Reset() {
            valueText = GetComponentInChildren<TMP_Text>(true);
            valueSlider = GetComponentInChildren<Slider>(true);
        }

        private void Awake() {
            valueSlider.minValue = 0;
            valueSlider.maxValue = 200;
            valueSlider.onValueChanged.AddListener(OnValueChanged);
            NetworkTransform.InterpolationBufferTickOffset = PlayerPrefs.GetInt(Key, 10);
        }

        private void OnEnable() {
            valueSlider.SetValueWithoutNotify(NetworkTransform.InterpolationBufferTickOffset);
            valueText.text = NetworkTransform.InterpolationBufferTickOffset.ToString();
        }

        private void OnValueChanged(float value) {
            NetworkTransform.InterpolationBufferTickOffset = Mathf.RoundToInt(value);
            valueText.text = NetworkTransform.InterpolationBufferTickOffset.ToString();
            PlayerPrefs.SetInt(Key, NetworkTransform.InterpolationBufferTickOffset);
        }
    }
}
