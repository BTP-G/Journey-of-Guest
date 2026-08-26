using UnityEngine;
using VContainer;
using Xoderony.Unity;

namespace Expriverse.UI {

    [RequireComponent(typeof(CanvasGroup))]
    public sealed class WorldTooltip : TextTooltip {
        [Inject] internal Billboarder billboarder;

        public void SetTooltip(IWorldTooltipSource source) {
            base.SetTooltip(source);
            transform.position = source.TooltipPosition;
        }

        public void SetPosition(Vector3 worldPosition) {
            transform.position = worldPosition;
        }

        public override void Show(float fadeIn = 0f) {
            base.Show(fadeIn);
            billboarder.Register(rectTransform);
        }

        public override void Hide(float fadeOut = 0) {
            base.Hide(fadeOut);
            billboarder.Unregister(rectTransform);
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            billboarder.Unregister(rectTransform);
        }
    }
}
