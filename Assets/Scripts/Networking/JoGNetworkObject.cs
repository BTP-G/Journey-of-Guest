using System.Collections.Generic;
using UnityEngine;
using Xoderony.Networking.Messaging;
using Xoderony.Networking.Serialization;

namespace JoG.Networking.P2P {
    /// <summary>JoG 对象协议：按登记顺序保存 NV，并直接持有 RPC channel handler。</summary>
    public class JoGNetworkObject : Xoderony.Networking.NetworkObject {
        private const int MaxVariableCount = byte.MaxValue + 1;
        private const int RpcChannelCount = byte.MaxValue + 1;

        private List<NetworkVariableBase> _variables;
        private NetworkMessageHandler[] _rpcHandlers;

        internal int VariableCount => _variables?.Count ?? 0;

        public void Register(NetworkVariableBase variable) {
            _variables ??= new List<NetworkVariableBase>();
            Debug.Assert(!_variables.Contains(variable), "Variable is already registered.");
            Debug.Assert(_variables.Count < MaxVariableCount, "Too many network variables.");
            _variables.Add(variable);
        }

        public void Register(byte channel, NetworkMessageHandler handler) {
            _rpcHandlers ??= new NetworkMessageHandler[RpcChannelCount];
            _rpcHandlers[channel] += handler;
        }

        public void Unregister(NetworkVariableBase variable) {
            Debug.Assert(_variables != null, "Variable is not registered.");
            var index = _variables.IndexOf(variable);
            Debug.Assert(index >= 0, "Variable is not registered.");
            _variables.RemoveAt(index);
        }

        public void Unregister(byte channel, NetworkMessageHandler handler) {
            Debug.Assert(_rpcHandlers != null, "RPC channel is not registered.");
            _rpcHandlers[channel] -= handler;
        }

        protected override void OnSerializeSnapshot(ref BufferWriter writer) {
            if (_variables != null) {
                for (var i = 0; i < _variables.Count; i++) {
                    _variables[i].Serialize(ref writer);
                }
            }

            base.OnSerializeSnapshot(ref writer);
        }

        protected override void OnDeserializeSnapshot(ref BufferReader reader) {
            if (_variables != null) {
                for (var i = 0; i < _variables.Count; i++) {
                    _variables[i].Deserialize(ref reader);
                }
            }

            base.OnDeserializeSnapshot(ref reader);
        }

        internal NetworkVariableBase GetVariable(int index) {
            return _variables[index];
        }

        internal void DeserializeVariable(int index, ref BufferReader reader) {
            Debug.Assert(index < _variables.Count, "State variable index is out of range.");
            _variables[index].Deserialize(ref reader);
        }

        internal void InvokeRpc(ulong senderPeerId, byte channel, BufferReader reader) {
            _rpcHandlers?[channel]?.Invoke(senderPeerId, reader);
        }
    }
}
