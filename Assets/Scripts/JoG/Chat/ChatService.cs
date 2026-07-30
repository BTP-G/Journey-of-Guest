using Xoderony.Extensions;
using Xoderony.Logging;
using JoG.Networking;
using System;
using Unity.Collections;
using Unity.Netcode;
using VContainer;
using VContainer.Unity;

namespace JoG.Chat {

    public class ChatService : IChatService, IStartable, IDisposable {
        [Inject] internal IPlayerRegistry _playerRegistry;
        [Inject] internal NetworkManager _networkManager;
        [Inject] internal UnnamedMessageBroker _messageBroker;

        public event ChatMessageHandler OnReceivedChatMessage;

        void IStartable.Start() {
            _messageBroker.RegisterMessageHandler(1, HandleChatMessage);
        }

        void IDisposable.Dispose() {
            _messageBroker.UnregisterMessageHandler(1, HandleChatMessage);
        }

        public unsafe void SendMessage(string message, byte type) {
            if (message.IsNullOrWhiteSpace()) {
                this.LogWarning("Message to send is null or white space.");
                return;
            }
            switch (type) {
                case ChatMessageTypes.System: {
                    if (!_networkManager.IsServer) {
                        this.LogWarning("Only server can send System message.");
                        return;
                    }
                }
                break;

                case ChatMessageTypes.Player: {
                    if (!_networkManager.IsClient) {
                        this.LogWarning("Only client can send Player message.");
                        return;
                    }
                }
                break;

                default:
                    this.LogWarning($"Unsupport message type: {type}");
                    break;
            }
            var bytes = message.Length * sizeof(char);
            if (bytes > 400) {
                bytes = 400;
                this.LogWarning("Message truncated to 200 characters.");
            }
            using var writer = new FastBufferWriter(bytes + 5, Allocator.Temp);
            writer.TryBeginWrite(bytes + 5);
            writer.WriteByte(type);
            writer.WriteValue(bytes);
            fixed (char* ptr = message) {
                writer.WriteBytes((byte*)ptr, bytes);
            }
            _messageBroker.SendMessageToOthers(1, writer, NetworkDelivery.Reliable);
        }

        private unsafe void HandleChatMessage(ulong clientId, FastBufferReader reader) {
            reader.TryBeginRead(5);
            reader.ReadByte(out byte chatMessageType);
            reader.ReadValue(out int bytes);
            var length = bytes / sizeof(char);
            var message = stackalloc char[length];
            reader.TryBeginRead(bytes);
            reader.ReadBytes((byte*)message, bytes);
            var messageSpan = new ReadOnlySpan<char>(message, length);
            OnReceivedChatMessage?.Invoke(clientId, chatMessageType, messageSpan);
        }
    }
}
