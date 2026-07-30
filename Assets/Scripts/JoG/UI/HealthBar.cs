using EditorAttributes;
using Xoderony.Extensions;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace JoG.UI {

    [Serializable]
    public class HealthBar {
        public const float Speed = 3f;
        [SerializeField, Required] private Image barImage;

        public void UpdateView(float healthRatio) {
            var current = barImage.fillAmount;
            if (current == healthRatio) return;
            barImage.fillAmount = current.MoveTowards(healthRatio, Speed * Time.deltaTime);
        }
    }
}
