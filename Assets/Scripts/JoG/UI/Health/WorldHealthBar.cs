using EditorAttributes;
using Xoderony.Extensions;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace JoG.UI.Health {

    [Serializable]
    public class WorldHealthBar {
        [SerializeField] private float speed = 3f;
        [Required, SerializeField] private Image barImage;

        public void UpdateView(float healthRatio) {
            var current = barImage.fillAmount;
            if (current == healthRatio) return;
            barImage.fillAmount = current.MoveTowards(healthRatio, speed * Time.deltaTime);
        }
    }
}
