using UnityEngine;
using UnityEngine.UI;

namespace DamageNumbersPro.Demo {
    public class DNP_SineFadeText : MonoBehaviour {
        public float fromAlpha = 0.5f;
        public float toAlpha = 0.8f;
        public float speed = 4f;
        public float startTimeBonus = 0f;

        private Text text;

        private void Awake() {
            text = GetComponent<Text>();
        }

        private void FixedUpdate() {
            var color = text.color;
            color.a = fromAlpha + ((toAlpha - fromAlpha) * ((Mathf.Sin((speed * Time.unscaledTime) + startTimeBonus) * 0.5f) + 0.5f));
            text.color = color;
        }
    }
}
