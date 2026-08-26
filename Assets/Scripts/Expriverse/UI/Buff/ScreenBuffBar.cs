using EditorAttributes;
using Expriverse.Character;
using Expriverse.GameplayEffects.Data;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Expriverse.UI.Buff {

    [Serializable]
    public class ScreenBuffBar {
        private readonly List<BuffIcon> _icons = new();
        [SerializeField, Required] private GameObject _buffIconTemplate;

        public void UpdateView(CharacterEffects effects) {
            var targetCount = GetDisplayedEffectCount(effects);
            var currentCount = _icons.Count;
            if (targetCount > currentCount) {
                var diff = targetCount - currentCount;
                for (var i = 0; i < diff; i++) {
                    var iconObject = UnityEngine.Object.Instantiate(_buffIconTemplate, _buffIconTemplate.transform.parent);
                    _icons.Add(new(iconObject));
                }
            } else if (targetCount < currentCount) {
                var removeStart = targetCount;
                for (var i = currentCount - 1; i >= removeStart; i--) {
                    _icons[i].gameObject.SetActive(false);
                }
            }
            var index = 0;
            foreach (var state in effects) {
                if (!state.Definition.TryGetData<EffectDisplayData>(out var displayData) || displayData.icon == null) {
                    continue;
                }
                var buffIcon = _icons[index];
                buffIcon.iconImage.sprite = displayData.icon;
                buffIcon.gameObject.SetActive(true);
                index++;
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

        private struct BuffIcon {
            public GameObject gameObject;
            public Image iconImage;

            public BuffIcon(GameObject go) {
                gameObject = go;
                iconImage = go.GetComponentInChildren<Image>();
            }
        }
    }
}
