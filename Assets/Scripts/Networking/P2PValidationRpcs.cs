using System.Collections.Generic;
using Xoderony.Logging;
using Xoderony.Networking;

namespace JoG.Networking.P2P {
    /// <summary>P2P 验证对象的 RPC 能力。</summary>
    public sealed class P2PValidationRpcs : NetworkRpcComponent {
        public readonly NetworkAllRpc<int> Broadcast = new();

        protected override void Awake() {
            base.Awake();
            Broadcast.Received = OnBroadcastReceived;
        }

        protected override void CollectNetworkRpcs(List<NetworkRpcBase> rpcs) {
            base.CollectNetworkRpcs(rpcs);
            rpcs.Add(Broadcast);
        }

        private void OnBroadcastReceived(ulong senderPeerId, in int value) {
            this.Log($"P2P validation RPC received from {senderPeerId}: {value}.");
        }
    }
}
