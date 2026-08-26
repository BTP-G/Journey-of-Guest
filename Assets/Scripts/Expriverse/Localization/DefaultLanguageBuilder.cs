using Expriverse.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VContainer.Unity;
using Xoderony.Localization;

namespace Expriverse.Localization {

    internal class DefaultLanguageBuilder : IInitializable, IDisposable {
        public const string FallbackLanguage = "zh-CN";

        void IInitializable.Initialize() {
            Localizer.LanguageBuilders += Build;
            Localizer.OnLanguageChanged += OnLanguageChanged;
            var defaultLanguage = Application.systemLanguage switch {
                SystemLanguage.Chinese or SystemLanguage.ChineseSimplified or SystemLanguage.ChineseTraditional => "zh-CN",
                _ => "en-US",
            };
            Localizer.LanguageCode = PlayerPrefs.GetString("language_code", defaultLanguage);
        }

        void IDisposable.Dispose() {
            Localizer.LanguageBuilders -= Build;
            Localizer.OnLanguageChanged -= OnLanguageChanged;
        }

        private void OnLanguageChanged() {
            PlayerPrefs.SetString("language_code", Localizer.LanguageCode);
        }

        private void Build(string languageCode, IDictionary<string, string> builder) {
            var path1 = Path.Combine(Application.streamingAssetsPath, "Localization", $"{FallbackLanguage}.hjson");
            AssetsUtility.LoadLanguageFromHjson(path1, builder);
            if (languageCode != FallbackLanguage) {
                var path2 = Path.Combine(Application.streamingAssetsPath, "Localization", $"{languageCode}.hjson");
                AssetsUtility.LoadLanguageFromHjson(path2, builder);
            }
        }
    }
}
