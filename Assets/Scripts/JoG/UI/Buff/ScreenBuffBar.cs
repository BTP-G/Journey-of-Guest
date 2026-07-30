using EditorAttributes;
using JoG.Character;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace JoG.UI.Buff {

    [Serializable]
    public class ScreenBuffBar {
        private readonly List<BuffIcon> _icons = new();
        [SerializeField, Required] private GameObject _buffIconTemplate;

        public void UpdateView(CharacterBuffs buffs) {
            var targetCount = buffs.Count;
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
            foreach (var buff in buffs) {
                var buffIcon = _icons[index];
                //buffIcon.iconImage.sprite = buff.Icon;
                //buffIcon.gameObject.SetActive(buff.Icon != null);
                index++;
            }
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
