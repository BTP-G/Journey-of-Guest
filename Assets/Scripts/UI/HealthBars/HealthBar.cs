using EditorAttributes;
using GuestUnion;
using UnityEngine;
using UnityEngine.UI;

namespace JoG.UI.HealthBars {

    public class HealthBar : MonoBehaviour {
        public const float LERP_T_COFFICIENT = 10f;
        [SerializeField, Required] protected Image barImage;
        public bool IsVisible { get; private set; }
        public Image BarImage => barImage;

        public virtual void UpdateView(float percent) {
            barImage.fillAmount = barImage.fillAmount.LerpToUnclamped(percent, LERP_T_COFFICIENT * Time.deltaTime);
        }

        private void OnBecameVisible() {
            IsVisible = true;
        }

        private void OnBecameInvisible() {
            IsVisible = false;
        }
    }
}