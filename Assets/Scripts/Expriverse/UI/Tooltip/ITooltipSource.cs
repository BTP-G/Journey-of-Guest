using System.Text;

namespace Expriverse.UI {

    public interface ITooltipSource {

        void BuildTooltip(StringBuilder builder);
    }
}
