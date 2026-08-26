using Cysharp.Threading.Tasks;
using Expriverse.Interaction;
using Expriverse.Networking;
using Expriverse.UI;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using VContainer;
using Xoderony.Localization;

namespace Expriverse.Props {

    [DisallowMultipleComponent]
    public class GameEndRock : NetworkBehaviour, IInteractable, IWorldTooltipSource {
        public Vector3 tooltipOffset;

        [LocalizationKey(@"^interact\..*\.name$")]
        public string nameKey;

        [LocalizationKey(@"^interact\..*\.desc$")]
        public string descriptionKey;

        [Inject] internal ISessionService sessionService;
        public Vector3 TooltipPosition => transform.position + tooltipOffset;

        public void BuildTooltip(StringBuilder builder) {
            builder.AppendLine(Localizer.GetString(nameKey))
                   .AppendLine(Localizer.GetString(descriptionKey));
        }

        public bool CanInteract(Entity interactor) {
            return enabled;
        }

        public void OnInteracted(Entity interactor) {
            sessionService.LeaveSessionAsync().Forget();
        }

        private void OnDrawGizmosSelected() {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, transform.position + tooltipOffset);
            Gizmos.DrawSphere(transform.position + tooltipOffset, 0.05f);
        }
    }
}
