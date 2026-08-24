using UnityEngine.Assertions;
using Xoderony.Networking;

namespace JoG.Networking.P2P {
    /// <summary>覆盖快照、网络变量和广播 RPC 的最小 P2P 验证对象。</summary>
    public class P2PValidationNetworkObject : NetworkObject {
        private P2PValidationVariables _variables;
        private P2PValidationRpcs _rpcs;

        protected override void Awake() {
            base.Awake();
            _variables = GetComponent<P2PValidationVariables>();
            _rpcs = GetComponent<P2PValidationRpcs>();
            Assert.IsNotNull(_variables, "P2P validation object is missing NetworkVariable capability.");
            Assert.IsNotNull(_rpcs, "P2P validation object is missing RPC capability.");
        }

        public void SetSnapshotValue(int value) {
            _variables.SnapshotValue.Value = value;
        }

        public void SendBroadcast(int value) {
            _rpcs.Broadcast.Send(value);
        }
    }
}
