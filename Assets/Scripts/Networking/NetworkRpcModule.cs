using System;
using UnityEngine.Assertions;
using VContainer.Unity;
using Xoderony.Networking;
using Xoderony.Networking.Messaging;
using Xoderony.Networking.Serialization;
using Xoderony.Networking.Transport;

namespace JoG.Networking.P2P {
    /// <summary>JoG 对象 RPC 协议；对象持有强类型端点，本模块只负责收发。</summary>
    public sealed class NetworkRpcModule : IInitializable, INetworkRpcSender, IDisposable {
        private readonly INetworkSession _session;
        private readonly INetworkMessageManager _messageManager;
        private readonly INetworkObjectManager _objectManager;

        public NetworkRpcModule(INetworkSession session, INetworkMessageManager messageManager, INetworkObjectManager objectManager) {
            _session = session;
            _messageManager = messageManager;
            _objectManager = objectManager;
        }

        public void SendToOthers(JoGNetworkObject networkObject, byte index, ReadOnlySpan<byte> payload) {
            Span<byte> buffer = stackalloc byte[NetworkMessageLimits.MessageCapacity];
            var writer = new BufferWriter(buffer);
            writer.WriteByte(NetworkObjectMessageType.Rpc);
            writer.WriteUInt(networkObject.Id);
            writer.WriteByte(index);
            writer.WriteBytes(payload);
            _messageManager.SendToOthers(writer.Written, NetworkDelivery.Reliable);
        }

        public void SendToAll(JoGNetworkObject networkObject, byte index, ReadOnlySpan<byte> payload) {
            Span<byte> buffer = stackalloc byte[NetworkMessageLimits.MessageCapacity];
            var writer = new BufferWriter(buffer);
            writer.WriteByte(NetworkObjectMessageType.Rpc);
            writer.WriteUInt(networkObject.Id);
            writer.WriteByte(index);
            writer.WriteBytes(payload);
            _messageManager.SendToAll(writer.Written, NetworkDelivery.Reliable);
        }

        public void SendToOwner(JoGNetworkObject networkObject, byte index, ReadOnlySpan<byte> payload) {
            if (networkObject.OwnerPeerId == _session.LocalPeerId) {
                var reader = new BufferReader(payload);
                Dispatch(networkObject, _session.LocalPeerId, index, ref reader);
                return;
            }

            SendToPeer(networkObject, networkObject.OwnerPeerId, index, payload);
        }

        public void SendToPeer(JoGNetworkObject networkObject, ulong peerId, byte index, ReadOnlySpan<byte> payload) {
            Span<byte> buffer = stackalloc byte[NetworkMessageLimits.MessageCapacity];
            var writer = new BufferWriter(buffer);
            writer.WriteByte(NetworkObjectMessageType.Rpc);
            writer.WriteUInt(networkObject.Id);
            writer.WriteByte(index);
            writer.WriteBytes(payload);
            _messageManager.SendToPeer(peerId, writer.Written, NetworkDelivery.Reliable);
        }

        void IInitializable.Initialize() {
            _messageManager.RegisterHandler(NetworkObjectMessageType.Rpc, OnRpcMessage);
        }

        public void Dispose() {
            _messageManager.UnregisterHandler(NetworkObjectMessageType.Rpc, OnRpcMessage);
        }

        private void OnRpcMessage(ulong senderPeerId, BufferReader reader) {
            var id = reader.ReadUInt();
            if (!_objectManager.TryGetSpawned(id, out var networkObject) || networkObject is not JoGNetworkObject jogNetworkObject) {
                return;
            }
            var index = reader.ReadByte();
            Dispatch(jogNetworkObject, senderPeerId, index, ref reader);
        }

        private static void Dispatch(JoGNetworkObject networkObject, ulong senderPeerId, byte index, ref BufferReader reader) {
            var rpcs = networkObject.NetworkRpcs;
            Assert.IsTrue(index < rpcs.Length, "RPC index is out of range.");
            rpcs[index].Deserialize(senderPeerId, ref reader);
        }
    }
}
