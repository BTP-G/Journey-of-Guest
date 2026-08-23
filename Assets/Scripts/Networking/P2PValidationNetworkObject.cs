using System.Collections.Generic;
using Xoderony.Logging;

namespace JoG.Networking.P2P {
    /// <summary>覆盖快照、网络变量和广播 RPC 的最小 P2P 验证对象。</summary>
    public class P2PValidationNetworkObject : JoGNetworkObject {
        public readonly NetworkVariable<int> SnapshotValue = new();
        public readonly NetworkAllRpc<int> BroadcastRpc = new();

        protected override void Awake() {
            base.Awake();
            BroadcastRpc.Received = OnBroadcastReceived;
        }

        protected override void CollectNetworkVariables(List<NetworkVariableBase> variables) {
            base.CollectNetworkVariables(variables);
            variables.Add(SnapshotValue);
        }

        protected override void CollectNetworkRpcs(List<NetworkRpcBase> rpcs) {
            base.CollectNetworkRpcs(rpcs);
            rpcs.Add(BroadcastRpc);
        }

        public void SetSnapshotValue(int value) {
            SnapshotValue.Value = value;
        }

        public void SendBroadcast(int value) {
            BroadcastRpc.Send(value);
        }

        private void OnBroadcastReceived(ulong senderPeerId, in int value) {
            this.Log($"P2P validation RPC received from {senderPeerId}: {value}.");
        }
    }
}
