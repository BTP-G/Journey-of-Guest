using System;
using Unity.Netcode;
using VContainer;
using VContainer.Unity;
using Xoderony.ObjectPool.Generic;

namespace Expriverse.Networking {

    public sealed class UnnamedMessageBroker : IStartable, IDisposable {
        private readonly NetworkManager _networkManager;
        private readonly CustomMessagingManager _messageManager;
        private readonly CustomMessagingManager.UnnamedMessageDelegate[] _handers = new CustomMessagingManager.UnnamedMessageDelegate[byte.MaxValue];

        [Inject]
        internal UnnamedMessageBroker(NetworkManager networkManager) {
            _networkManager = networkManager;
            _messageManager = networkManager.CustomMessagingManager;
        }

        void IStartable.Start() {
            _messageManager.OnUnnamedMessage += OnReceviedUnnamedMessage;
        }

        void IDisposable.Dispose() {
            _messageManager.OnUnnamedMessage -= OnReceviedUnnamedMessage;
        }

        public void RegisterMessageHandler(byte messageType, CustomMessagingManager.UnnamedMessageDelegate hander) {
            _handers[messageType] += hander;
        }

        public void UnregisterMessageHandler(byte messageType, CustomMessagingManager.UnnamedMessageDelegate hander) {
            _handers[messageType] -= hander;
        }

        public void SendMessageToOthers(byte messageType, FastBufferWriter messageBuffer, NetworkDelivery networkDelivery) {
            var senderClientId = _networkManager.LocalClientId;
            var size = messageBuffer.Length + 13;
            using var writer = new FastBufferWriter(size, Unity.Collections.Allocator.Temp);
            writer.TryBeginWrite(size);
            writer.WriteByte(messageType);
            writer.WriteValue(senderClientId);
            writer.WriteValue(messageBuffer.Length);
            writer.CopyFrom(messageBuffer);
            if (_networkManager.IsServer) {
                using (ListPool<ulong>.Rent(out var targetClientIds)) {
                    foreach (var client in _networkManager.ConnectedClientsList) {
                        if (client.ClientId == senderClientId) {
                            continue;
                        }

                        targetClientIds.Add(client.ClientId);
                    }
                    _messageManager.SendUnnamedMessage(targetClientIds, writer, networkDelivery);
                }
            } else {
                _messageManager.SendUnnamedMessage(NetworkManager.ServerClientId, writer, networkDelivery);
            }
        }

        private unsafe void OnReceviedUnnamedMessage(ulong clientId, FastBufferReader reader) {
            reader.TryBeginRead(13);
            reader.ReadByte(out var messageType);
            reader.ReadValue(out ulong senderClientId);
            reader.ReadValue(out int contentSize);
            _handers[messageType]?.Invoke(senderClientId, reader);
            if (_networkManager.IsServer) {
                var writer = new FastBufferWriter(contentSize + 13, Unity.Collections.Allocator.Temp);
                writer.WriteByte(messageType);
                writer.WriteValue(senderClientId);
                writer.WriteBytes(reader.GetUnsafePtrAtCurrentPosition(), contentSize);
                using (ListPool<ulong>.Rent(out var targetClientIds)) {
                    foreach (var client in _networkManager.ConnectedClientsList) {
                        if (client.ClientId == senderClientId || client.ClientId == _networkManager.LocalClientId) {
                            continue;
                        }

                        targetClientIds.Add(client.ClientId);
                    }
                    _messageManager.SendUnnamedMessage(targetClientIds, writer, NetworkDelivery.Reliable);
                }
            }
        }
    }
}
