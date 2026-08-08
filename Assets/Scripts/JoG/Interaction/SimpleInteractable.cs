using JoG.UI;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using Xoderony.Localization;

namespace JoG.Interaction {

    public sealed class SimpleInteractable : MonoBehaviour, IInteractable, IWorldTooltipSource {

        [LocalizationKey]
        public string nameKey;

        [LocalizationKey]
        public string descriptionKey;

        public Vector3 tooltipOffset;
        [field: SerializeField] public UnityEvent OnInteracted { get; private set; }

        public Vector3 TooltipPosition => transform.position + tooltipOffset;

        void ITooltipSource.BuildTooltip(StringBuilder builder) {
            builder.AppendLine(Localizer.GetString(nameKey))
                   .AppendLine(Localizer.GetString(descriptionKey));
        }

        bool IInteractable.CanInteract(Entity entity) {
            return true;
        }

        void IInteractable.OnInteracted(Entity entity) {
            OnInteracted.Invoke();
        }

        private void OnDrawGizmosSelected() {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, transform.position + tooltipOffset);
            Gizmos.DrawSphere(transform.position + tooltipOffset, 0.05f);
        }
    }
}
