using UnityEngine;

namespace JoG.UI {

    public interface IWorldTooltipSource : ITooltipSource {
        Vector3 TooltipPosition { get; }
    }
}
