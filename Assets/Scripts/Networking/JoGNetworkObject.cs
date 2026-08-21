using System.Collections.Generic;
using UnityEngine.Assertions;
using Xoderony.Networking.Messaging;
using Xoderony.Networking.Serialization;

namespace JoG.Networking.P2P {
    /// <summary>JoG 对象协议：按登记顺序保存 NV，并直接持有 RPC channel handler。</summary>
    public class JoGNetworkObject : Xoderony.Networking.NetworkObject {
        private const int MaxVariableCount = byte.MaxValue + 1;
        private const int RpcChannelCount = byte.MaxValue + 1;

        private readonly List<NetworkVariableBase> _variables = new List<NetworkVariableBase>();
        private readonly NetworkMessageHandler[] _rpcHandlers = new NetworkMessageHandler[RpcChannelCount];

        internal int VariableCount => _variables.Count;

        public void Register(NetworkVariableBase variable) {
            Assert.IsFalse(_variables.Contains(variable), "Variable is already registered.");
            Assert.IsTrue(_variables.Count < MaxVariableCount, "Too many network variables.");
            _variables.Add(variable);
        }

        public void Register(byte channel, NetworkMessageHandler handler) {
            _rpcHandlers[channel] += handler;
        }

        public void Unregister(NetworkVariableBase variable) {
            var index = _variables.IndexOf(variable);
            Assert.AreNotEqual(-1, index, "Variable is not registered.");
            _variables.RemoveAt(index);
        }

        public void Unregister(byte channel, NetworkMessageHandler handler) {
            _rpcHandlers[channel] -= handler;
        }

        protected override void OnSerializeSnapshot(ref BufferWriter writer) {
            for (var i = 0; i < _variables.Count; i++) {
                _variables[i].Serialize(ref writer);
            }

            base.OnSerializeSnapshot(ref writer);
        }

        protected override void OnDeserializeSnapshot(ref BufferReader reader) {
            for (var i = 0; i < _variables.Count; i++) {
                _variables[i].Deserialize(ref reader);
            }

            base.OnDeserializeSnapshot(ref reader);
        }

        internal NetworkVariableBase GetVariable(int index) {
            return _variables[index];
        }

        internal void DeserializeVariable(int index, ref BufferReader reader) {
            Assert.IsTrue(index < _variables.Count, "State variable index is out of range.");
            _variables[index].Deserialize(ref reader);
        }

        internal void InvokeRpc(ulong senderPeerId, byte channel, BufferReader reader) {
            _rpcHandlers[channel]?.Invoke(senderPeerId, reader);
        }
    }
}
