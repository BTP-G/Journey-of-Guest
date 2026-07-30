using Xoderony.Localization;
using JoG.Localization;

namespace JoG.Gameplay.Objectives {

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
