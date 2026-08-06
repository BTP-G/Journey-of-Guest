using EditorAttributes;
using JoG.Character;
using JoG.Gameplay.Effects.Data;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace JoG.UI.Buff {

    [Serializable]
    public class WorldBuffBar {
        private readonly List<Image> _iconImages = new();
        [Required, SerializeField] private Image iconImageTemplate;

        public void UpdateView(CharacterEffects effects) {
            var targetCount = GetDisplayedEffectCount(effects);
            var currentCount = _iconImages.Count;
            if (targetCount > currentCount) {
                var diff = targetCount - currentCount;
                for (var i = 0; i < diff; i++) {
                    var item = UnityEngine.Object.Instantiate(iconImageTemplate, iconImageTemplate.transform.parent);
                    _iconImages.Add(item);
                }
            } else if (targetCount < currentCount) {
                var removeStart = targetCount;
                for (var i = currentCount - 1; i >= removeStart; i--) {
                    _iconImages[i].gameObject.SetActive(false);
                }
            }
            var index = 0;
            foreach (var state in effects) {
                if (!state.Definition.TryGetData<EffectDisplayData>(out var displayData) || displayData.icon == null) {
                    continue;
                }
                var buffIcon = _iconImages[index++];
                buffIcon.sprite = displayData.icon;
                buffIcon.gameObject.SetActive(true);
            }
        }

        private static int GetDisplayedEffectCount(CharacterEffects effects) {
            var count = 0;
            foreach (var state in effects) {
                if (state.Definition.TryGetData<EffectDisplayData>(out var displayData) && displayData.icon != null) {
                    count++;
                }
            }
            return count;
        }
    }
}
