using System.Text;
using TMPro;

namespace JoG.UI {

    public class TextMeshTooltipTrigger : TooltipTrigger {
        public TMP_Text tooltipSourceTextMesh;

        public override void BuildTooltip(StringBuilder builder) {
            builder.Append(tooltipSourceTextMesh.text);
        }

        protected override void Reset() {
            base.Reset();
            tooltipSourceTextMesh = GetComponentInChildren<TMP_Text>();
        }
    }
}
