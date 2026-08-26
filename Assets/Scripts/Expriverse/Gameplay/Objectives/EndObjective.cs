using Expriverse.Localization;
using Xoderony.Localization;

namespace Expriverse.Gameplay.Objectives {

    public class EndObjective : ObjectiveController {

        public override void OnNetworkSpawn() {
            base.OnNetworkSpawn();
            UpdateText();
        }

        private void UpdateText() {
            labelText.text = Localizer.GetString(L10nKeys.Objective.EndObjective);
        }
    }
}
