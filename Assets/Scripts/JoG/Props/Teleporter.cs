using EditorAttributes;
using Xoderony.Localization;
using JoG.Gameplay;
using JoG.Interaction;
using JoG.Localization;
using JoG.UI;
using System;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JoG.Assets.JoG.Props {

    [DisallowMultipleComponent]
    public class Teleporter : NetworkBehaviour, IInteractable, IWorldTooltipSource {

        [SceneDropdown]
        public string nextSceneName;

        public ObjectiveController[] requiredObjective = Array.Empty<ObjectiveController>();
        public SceneEventProgressStatus status;
        public Vector3 tooltipOffset;

        [LocalizationKey(@"^interact\..*\.name$")]
        public string nameKey;

        [LocalizationKey(@"^interact\..*\.desc$")]
        public string descriptionKey;

        public bool Active {
            get {
                foreach (var objective in requiredObjective) {
                    if (objective.IsComplete) continue;
                    return false;
                }
                return true;
            }
        }

        public Vector3 TooltipPosition => transform.position + tooltipOffset;

        public void BuildTooltip(StringBuilder builder) {
            var statusString = Active ? L10nKeys.Interact.Status.Active : L10nKeys.Interact.Status.Inactive;
            builder.AppendLine(Localizer.GetString(statusString))
                   .AppendLine(Localizer.GetString(nameKey))
                   .AppendLine(Localizer.GetString(descriptionKey));
            foreach (var objective in requiredObjective) {
                builder.Append('·')
                    .AppendLine(objective.labelText.text);
            }
        }

        public bool CanInteract(Entity interactor) {
            return Active;
        }

        public void OnInteracted(Entity interactor) {
            LoadNextSceneRpc();
        }

        private void OnDrawGizmosSelected() {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, transform.position + tooltipOffset);
            Gizmos.DrawSphere(transform.position + tooltipOffset, 0.05f);
        }

        [Rpc(SendTo.Authority)]
        private void LoadNextSceneRpc() {
            if (status == SceneEventProgressStatus.Started) return;
            status = NetworkManager.SceneManager.LoadScene(nextSceneName, LoadSceneMode.Single);
        }
    }
}
