using System;
using VContainer.Unity;
using Xoderony.Networking;
using Xoderony.Networking.Messaging;
using Xoderony.Networking.Serialization;
using Xoderony.Networking.Transport;

namespace JoG.Networking.P2P {
    /// <summary>JoG 对象 RPC 协议；对象直接持有 channel handler，本模块只负责收发。</summary>
    public sealed class NetworkRpcModule : IInitializable, INetworkRpcSender, IDisposable {
        private readonly INetworkSession _session;
        private readonly INetworkMessageManager _messageManager;
        private readonly INetworkObjectManager _objectManager;

        public NetworkRpcModule(INetworkSession session, INetworkMessageManager messageManager, INetworkObjectManager objectManager) {
            _session = session;
            _messageManager = messageManager;
            _objectManager = objectManager;
        }

        public void SendToOthers(JoGNetworkObject networkObject, byte channel, ReadOnlySpan<byte> payload, NetworkDelivery delivery = NetworkDelivery.Reliable) {
            Span<byte> buffer = stackalloc byte[NetworkMessageLimits.MessageCapacity];
            var writer = new BufferWriter(buffer);
            writer.WriteByte(NetworkObjectMessageType.Rpc);
            writer.WriteUInt(networkObject.Id);
            writer.WriteByte(channel);
            writer.WriteBytes(payload);
            _messageManager.SendToOthers(writer.Written, delivery);
        }

        public void SendToAll(JoGNetworkObject networkObject, byte channel, ReadOnlySpan<byte> payload, NetworkDelivery delivery = NetworkDelivery.Reliable) {
            SendToOthers(networkObject, channel, payload, delivery);
            networkObject.RpcHandlers[channel]?.Invoke(_session.LocalPeerId, new BufferReader(payload));
        }

        public void SendToPeer(JoGNetworkObject networkObject, ulong peerId, byte channel, ReadOnlySpan<byte> payload, NetworkDelivery delivery = NetworkDelivery.Reliable) {
            Span<byte> buffer = stackalloc byte[NetworkMessageLimits.MessageCapacity];
            var writer = new BufferWriter(buffer);
            writer.WriteByte(NetworkObjectMessageType.Rpc);
            writer.WriteUInt(networkObject.Id);
            writer.WriteByte(channel);
            writer.WriteBytes(payload);
            _messageManager.SendToPeer(peerId, writer.Written, delivery);
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
            var channel = reader.ReadByte();
            jogNetworkObject.RpcHandlers[channel]?.Invoke(senderPeerId, reader);
        }
    }
}
