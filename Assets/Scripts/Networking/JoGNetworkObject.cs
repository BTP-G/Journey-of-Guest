using System;
using System.Collections.Generic;
using UnityEngine.Assertions;
using VContainer;
using Xoderony.Networking.Messaging;
using Xoderony.Networking.Serialization;
using Xoderony.Networking.Transport;
using Xoderony.ObjectPool.Generic;

namespace JoG.Networking.P2P {
    /// <summary>JoG 对象协议：在 Awake 中固定收集 NV 与 RPC handler。</summary>
    public class JoGNetworkObject : Xoderony.Networking.NetworkObject {
        private const int MaxVariableCount = byte.MaxValue + 1;
        private const int RpcChannelCount = byte.MaxValue + 1;

        [Inject] private INetworkRpcSender _rpcSender;
        [Inject] private INetworkVariableScheduler _networkVariableScheduler;

        private NetworkVariableBase[] _networkVariables = Array.Empty<NetworkVariableBase>();
        private readonly NetworkMessageHandler[] _rpcHandlers = new NetworkMessageHandler[RpcChannelCount];

        internal ReadOnlySpan<NetworkVariableBase> NetworkVariables => _networkVariables;

        internal ReadOnlySpan<NetworkMessageHandler> RpcHandlers => _rpcHandlers;

        protected virtual void Awake() {
            using (ListPool<NetworkVariableBase>.Rent(out var variables)) {
                CollectNetworkVariables(variables);
                Assert.IsTrue(variables.Count <= MaxVariableCount, "Too many network variables.");
                for (var i = 0; i < variables.Count; i++) {
                    Assert.AreEqual(i, variables.IndexOf(variables[i]), "Network variable is collected more than once.");
                }
                _networkVariables = variables.ToArray();
            }

            for (var i = 0; i < _networkVariables.Length; i++) {
                _networkVariables[i].Bind(this);
            }

            CollectRpcHandlers(_rpcHandlers);
        }

        /// <summary>派生类重写此方法，以声明顺序收集网络变量。</summary>
        protected virtual void CollectNetworkVariables(List<NetworkVariableBase> variables) {
        }

        /// <summary>派生类重写此方法，以固定 channel 收集 RPC handler。</summary>
        protected virtual void CollectRpcHandlers(NetworkMessageHandler[] handlers) {
        }

        public void SendRpcToOthers(byte channel, ReadOnlySpan<byte> payload, NetworkDelivery delivery = NetworkDelivery.Reliable) {
            _rpcSender.SendToOthers(this, channel, payload, delivery);
        }

        public void SendRpcToAll(byte channel, ReadOnlySpan<byte> payload, NetworkDelivery delivery = NetworkDelivery.Reliable) {
            _rpcSender.SendToAll(this, channel, payload, delivery);
        }

        public void SendRpcToPeer(ulong peerId, byte channel, ReadOnlySpan<byte> payload, NetworkDelivery delivery = NetworkDelivery.Reliable) {
            _rpcSender.SendToPeer(this, peerId, channel, payload, delivery);
        }

        internal void MarkNetworkVariableDirty() {
            _networkVariableScheduler.Schedule(this);
        }

        protected override void OnSerializeSnapshot(ref BufferWriter writer) {
            for (var i = 0; i < _networkVariables.Length; i++) {
                _networkVariables[i].Serialize(ref writer);
            }

            base.OnSerializeSnapshot(ref writer);
        }

        protected override void OnDeserializeSnapshot(ref BufferReader reader) {
            for (var i = 0; i < _networkVariables.Length; i++) {
                _networkVariables[i].Deserialize(ref reader);
            }

            base.OnDeserializeSnapshot(ref reader);
        }
    }
}
