using Cysharp.Text;
using EditorAttributes;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Xoderony.Extensions;

namespace JoG.UI.Health {

    [Serializable]
    public class ScreenHealthBar {
        [SerializeField] private float speed = 3f;
        [Required, SerializeField] private Image barImage;
        [Required, SerializeField] private TextMeshProUGUI healthText;
        private int cachedCurrentHealth = -1;
        private int cachedMaxHealth = -1;

        public void UpdateView(int currentHealth, int maxHealth) {
            if (cachedCurrentHealth != currentHealth || cachedMaxHealth != maxHealth) {
                using var sb = ZString.CreateStringBuilder(true);
                sb.Append(currentHealth);
                sb.Append('/');
                sb.Append(maxHealth);
                healthText.SetText(sb);
                cachedCurrentHealth = currentHealth;
                cachedMaxHealth = maxHealth;
            }
            var healthRatio = (float)currentHealth / maxHealth;
            UpdateView(healthRatio);
        }

        private void UpdateView(float healthRatio) {
            var current = barImage.fillAmount;
            if (current == healthRatio) {
                return;
            }

            barImage.fillAmount = current.MoveTowards(healthRatio, speed * Time.deltaTime);
        }
    }
}
