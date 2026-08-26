using System.Text;
using Xoderony.Localization;

namespace Expriverse.UI {

    public class LocalizationTooltipTrigger : TooltipTrigger {

        [LocalizationKey]
        public string tooltipKey;

        public override void BuildTooltip(StringBuilder builder) {
            builder.Append(Localizer.GetString(tooltipKey));
        }
    }
}
