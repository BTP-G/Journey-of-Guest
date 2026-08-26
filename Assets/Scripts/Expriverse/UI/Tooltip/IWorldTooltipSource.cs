using UnityEngine;

namespace Expriverse.UI {

    public interface IWorldTooltipSource : ITooltipSource {
        Vector3 TooltipPosition { get; }
    }
}
