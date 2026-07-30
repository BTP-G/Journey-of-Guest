using System.Text;

namespace JoG.UI {

    public interface ITooltipSource {

        void BuildTooltip(StringBuilder builder);
    }
}
