using EditorAttributes;
using JoG.Character;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace JoG.UI.Buff {

    [Serializable]
    public class WorldBuffBar {
        private readonly List<Image> _iconImages = new();
        [Required, SerializeField] private Image iconImageTemplate;

        public void UpdateView(CharacterBuffs buffs) {
            var targetCount = buffs.Count;
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
            foreach (var buff in buffs) {
                var buffIcon = _iconImages[index++];
                //buffIcon.sprite = buff.Icon;
                //buffIcon.gameObject.SetActive(buff.Icon != null);
            }
        }
    }
}
