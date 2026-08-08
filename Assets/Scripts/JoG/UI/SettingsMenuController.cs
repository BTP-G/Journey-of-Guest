using JoG.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using Xoderony.Localization;

namespace JoG.UI {

    public class SettingsMenuController : MonoBehaviour {
        public AudioMixer audioMixer;
        public Slider masterVolumeSlider;
        public Slider musicVolumeSlider;
        public Slider effectVolumeSlider;
        public TMP_Dropdown qualityLevelDropdown;
        public TMP_Dropdown fullScreenModeDropdown;

        public void SetMasterVolume(float value) {
            PlayerPrefs.SetFloat("master_volume", value);
            var dB = value > 0 ? Mathf.Log10(value) * 20 : -80;
            audioMixer.SetFloat("MasterVolume", dB);
        }

        public void SetMusicVolume(float value) {
            PlayerPrefs.SetFloat("music_volume", value);
            var dB = value > 0 ? Mathf.Log10(value) * 20 : -80;
            audioMixer.SetFloat("MusicVolume", dB);
        }

        public void SetEffectVolume(float value) {
            PlayerPrefs.SetFloat("effect_volume", value);
            var dB = value > 0 ? Mathf.Log10(value) * 20 : -80;
            audioMixer.SetFloat("EffectVolume", dB);
        }

        public void SetQualityLevel(int index) {
            PlayerPrefs.SetInt("quality_level", index);
            QualitySettings.SetQualityLevel(1);
        }

        public void SetFullScreenMode(int fullScreenMode) {
            PlayerPrefs.SetInt("full_screen_mode", fullScreenMode);
            Screen.fullScreenMode = (FullScreenMode)fullScreenMode;
        }

        private void Awake() {
            masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
            musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
            effectVolumeSlider.onValueChanged.AddListener(SetEffectVolume);
            qualityLevelDropdown.onValueChanged.AddListener(SetQualityLevel);
            fullScreenModeDropdown.onValueChanged.AddListener(SetFullScreenMode);
            qualityLevelDropdown.options = new() {
                new() { text = Localizer.GetString(L10nKeys.Settings.Graphic.QualityLevel.Low) },
                new() { text = Localizer.GetString(L10nKeys.Settings.Graphic.QualityLevel.Medium) },
                new() { text = Localizer.GetString(L10nKeys.Settings.Graphic.QualityLevel.High) },
            };
            fullScreenModeDropdown.options = new() {
                new() { text = Localizer.GetString(L10nKeys.Settings.General.FullScreenMode.ExclusiveFullScreen) },
                new() { text = Localizer.GetString(L10nKeys.Settings.General.FullScreenMode.FullScreenWindow) },
                new() { text = Localizer.GetString(L10nKeys.Settings.General.FullScreenMode.MaximizedWindow) },
                new() { text = Localizer.GetString(L10nKeys.Settings.General.FullScreenMode.Windowed) },
            };
            masterVolumeSlider.value = PlayerPrefs.GetFloat("master_volume", 1);
            musicVolumeSlider.value = PlayerPrefs.GetFloat("music_volume", 1);
            effectVolumeSlider.value = PlayerPrefs.GetFloat("effect_volume", 1);
            qualityLevelDropdown.value = PlayerPrefs.GetInt("quality_level", 1);
            fullScreenModeDropdown.value = PlayerPrefs.GetInt("full_screen_mode", 1);
        }
    }
}
