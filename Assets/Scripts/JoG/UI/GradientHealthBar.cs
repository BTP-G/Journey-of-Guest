using EditorAttributes;
using Xoderony.Extensions;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace JoG.UI {

    [Serializable]
    public class GradientHealthBar {
        public const float Speed = 3f;
        public Gradient gradient;
        [SerializeField, Required] protected Image barImage;

        public void FillAmount(float healthRatio) {
            var current = barImage.fillAmount;
            if (current == healthRatio) return;
            barImage.fillAmount = current.MoveTowards(healthRatio, Speed * Time.deltaTime);
            barImage.color = gradient.Evaluate(barImage.fillAmount);
        }
    }
}
