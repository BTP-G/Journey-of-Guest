using System;
using UnityEngine;
using VContainer.Unity;
using Xoderony.Networking;
using Xoderony.Networking.Messaging;
using Xoderony.Networking.Serialization;
using Xoderony.Networking.Transport;

namespace JoG.Networking.P2P {
    /// <summary>JoG 对象 RPC 协议；对象直接持有 channel handler，本模块只负责收发。</summary>
    public sealed class NetworkRpcModule : IInitializable, IDisposable {
        private readonly INetworkManager _networkManager;
        private readonly INetworkObjectResolver _objectResolver;

        public NetworkRpcModule(INetworkManager networkManager, INetworkObjectResolver objectResolver) {
            _networkManager = networkManager;
            _objectResolver = objectResolver;
        }

        public void SendToOthers(JoGNetworkObject networkObject, byte channel, ReadOnlySpan<byte> payload, NetworkDelivery delivery = NetworkDelivery.Reliable) {
            Debug.Assert(networkObject.IsOwner, "Only the spawned owner can send object RPC.");
            Span<byte> buffer = stackalloc byte[NetworkMessageLimits.PayloadCapacity];
            var writer = new BufferWriter(buffer);
            writer.WriteUInt(networkObject.Id.Sequence);
            writer.WriteByte(channel);
            writer.WriteBytes(payload);
            _networkManager.SendToOthers(NetworkObjectMessageType.Rpc, writer.Written, delivery);
        }

        public void SendToAll(JoGNetworkObject networkObject, byte channel, ReadOnlySpan<byte> payload, NetworkDelivery delivery = NetworkDelivery.Reliable) {
            SendToOthers(networkObject, channel, payload, delivery);
            networkObject.InvokeRpc(_networkManager.LocalPeerId, channel, new BufferReader(payload));
        }

        public void SendToPeer(JoGNetworkObject networkObject, ulong peerId, byte channel, ReadOnlySpan<byte> payload, NetworkDelivery delivery = NetworkDelivery.Reliable) {
            Debug.Assert(networkObject.IsOwner, "Only the spawned owner can send object RPC.");
            Span<byte> buffer = stackalloc byte[NetworkMessageLimits.PayloadCapacity];
            var writer = new BufferWriter(buffer);
            writer.WriteUInt(networkObject.Id.Sequence);
            writer.WriteByte(channel);
            writer.WriteBytes(payload);
            _networkManager.SendToPeer(peerId, NetworkObjectMessageType.Rpc, writer.Written, delivery);
        }

        void IInitializable.Initialize() {
            _networkManager.Started += OnSessionStarted;
            _networkManager.Stopped += OnSessionStopped;
        }

        public void Dispose() {
            _networkManager.Started -= OnSessionStarted;
            _networkManager.Stopped -= OnSessionStopped;
            _networkManager.UnregisterMessage(NetworkObjectMessageType.Rpc, OnRpcMessage);
        }

        private void OnSessionStarted() {
            _networkManager.RegisterMessage(NetworkObjectMessageType.Rpc, OnRpcMessage);
        }

        private void OnSessionStopped() {
            _networkManager.UnregisterMessage(NetworkObjectMessageType.Rpc, OnRpcMessage);
        }

        private void OnRpcMessage(ulong senderPeerId, BufferReader reader) {
            var id = new NetworkObjectId(senderPeerId, reader.ReadUInt());
            if (!_objectResolver.TryGetSpawned(id, out var networkObject) || networkObject is not JoGNetworkObject jogNetworkObject) {
                return;
            }

            var channel = reader.ReadByte();
            jogNetworkObject.InvokeRpc(senderPeerId, channel, reader);
        }
    }
}
