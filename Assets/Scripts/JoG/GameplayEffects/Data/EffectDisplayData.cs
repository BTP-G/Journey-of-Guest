using EditorAttributes;
using System;
using UnityEngine;
using Xoderony.GameplayEffects;
using Xoderony.Localization;

namespace JoG.GameplayEffects.Data {

    [Serializable]
    public sealed class EffectDisplayData : GameplayEffectData {

        [AssetPreview(100, 100, order = -1)]
        public Sprite icon;

        [LocalizationKey(@"^buff\..*\.name$")]
        public string nameKey;

        [LocalizationKey(@"^buff\..*\.desc$")]
        public string descKey;
    }
}
