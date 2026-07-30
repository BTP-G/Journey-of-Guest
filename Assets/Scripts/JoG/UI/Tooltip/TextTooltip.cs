using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using System.Text;
using TMPro;
using UnityEngine;

namespace JoG.UI {

    [RequireComponent(typeof(CanvasGroup))]
    public abstract class TextTooltip : MonoBehaviour {
        protected RectTransform rectTransform;
        private readonly StringBuilder _tooltipBuilder = new(128);
        private TMP_Text tooltipText;
        private TweenerCore<float, float, FloatOptions> _tweener;

        public RectTransform RectTransform => rectTransform;

        public string Tooltip {
            get => tooltipText.text;
            set {
                tooltipText.text = value;
                var size = tooltipText.GetPreferredValues(1920, 1080);
                rectTransform.sizeDelta = size;
            }
        }

        public void SetTooltip(char[] tooltip, int start, int length) {
            tooltipText.SetText(tooltip, start, length);
            var size = tooltipText.GetPreferredValues(1920, 1080);
            rectTransform.sizeDelta = size;
        }

        public void SetTooltip(ITooltipSource source) {
            source.BuildTooltip(_tooltipBuilder.Clear());
            tooltipText.SetText(_tooltipBuilder);
            var size = tooltipText.GetPreferredValues(1920, 1080);
            rectTransform.sizeDelta = size;
        }

        public virtual void Show(float fadeIn = 0f) {
            _tweener.ChangeEndValue(1f, fadeIn, true).Play();
        }

        public virtual void Hide(float fadeOut = 0f) {
            _tweener.ChangeEndValue(0f, fadeOut, true).Play();
        }

        protected virtual void Awake() {
            var group = GetComponent<CanvasGroup>();
            tooltipText = GetComponentInChildren<TMP_Text>();
            rectTransform = transform as RectTransform;
            rectTransform.anchorMax = Vector2.zero;
            rectTransform.anchorMin = Vector2.zero;
            _tweener = group.DOFade(1, 1)
                .From(0)
                .Pause()
                .SetEase(Ease.OutQuad)
                .SetAutoKill(false);
        }

        protected virtual void OnDestroy() {
            _tweener.Kill();
        }
    }
}
