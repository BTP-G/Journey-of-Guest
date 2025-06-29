using UnityEngine;

namespace JoG.UI.HealthBars {

    public class GradientHealthBar : HealthBar {
        public Gradient gradient;

        public override void UpdateView(float percent) {
            base.UpdateView(percent);
            barImage.color = gradient.Evaluate(barImage.fillAmount);
        }
    }
}