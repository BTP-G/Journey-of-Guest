using Xoderony.Localization;
using JoG.Localization;
using System.Text;

namespace JoG.UI {

    public class LocalizationTooltipTrigger : TooltipTrigger {

        [LocalizationKey]
        public string tooltipKey;

        public override void BuildTooltip(StringBuilder builder) {
            builder.Append(Localizer.GetString(tooltipKey));
        }
    }
}
