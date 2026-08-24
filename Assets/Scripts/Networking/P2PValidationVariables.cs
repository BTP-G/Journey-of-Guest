using System.Collections.Generic;
using Xoderony.Networking;

namespace JoG.Networking.P2P {
    /// <summary>P2P 验证对象的 NetworkVariable 能力。</summary>
    public sealed class P2PValidationVariables : NetworkVariableComponent {
        public readonly NetworkVariable<int> SnapshotValue = new();

        protected override void CollectNetworkVariables(List<NetworkVariableBase> variables) {
            base.CollectNetworkVariables(variables);
            variables.Add(SnapshotValue);
        }
    }
}
