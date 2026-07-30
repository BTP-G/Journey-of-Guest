using EditorAttributes;
using Xoderony.Localization;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;

namespace JoG.Localization {

    public class LanguageDropdown : MonoBehaviour {
        [Required] public TMP_Dropdown languageDropdown;

        private void Reset() => languageDropdown = GetComponentInChildren<TMP_Dropdown>(true);

        private void Awake() {
            languageDropdown.options.Add(new TMP_Dropdown.OptionData("简体中文"));
            languageDropdown.options.Add(new TMP_Dropdown.OptionData("English"));
            languageDropdown.onValueChanged.AddListener(OnLanguageSelected);
        }

        private void OnEnable() => languageDropdown.SetValueWithoutNotify(LanguageCodeToIndex(Localizer.LanguageCode));

        private void OnLanguageSelected(int index) => Localizer.LanguageCode = IndexToLanguageCode(index);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string IndexToLanguageCode(int index) => index switch {
            0 => "zh-CN",
            _ => "en-US"
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int LanguageCodeToIndex(string languageCode) => languageCode switch {
            "zh-CN" => 0,
            _ => 1,
        };
    }
}
