using JoG.Networking;
using System;
using Unity.Collections;
using Unity.Netcode;
using VContainer;
using VContainer.Unity;
using Xoderony;
using Xoderony.Logging;

namespace JoG.Health {

    public sealed class HitRouter : IStartable, IDisposable {

        private const byte MessageType = 3;

        [Inject]
        internal UnnamedMessageBroker unnamedMessageBroker;

        public void Start() {
            unnamedMessageBroker.RegisterMessageHandler(MessageType, HandleMessage);
        }

        public void Dispose() {
            unnamedMessageBroker.UnregisterMessageHandler(MessageType, HandleMessage);
        }

        public void Route(Entity source, Entity target, in HitMessage message) {
            var outgoingHandlers = source?.GetComponent<IDelegateDispatcher<OutgoingHitMessageHandler>>();
            var incomingHandlers = target.GetComponent<IDelegateDispatcher<IncomingHitMessageHandler>>();
            outgoingHandlers?.Handlers?.Invoke(message, target);
            incomingHandlers.Handlers?.Invoke(message, source);
        }

        public void Broadcast(Entity source, Entity target, in HitMessage message) {
            using var writer = new FastBufferWriter(64, Allocator.Temp);
            writer.WriteValueSafe(source);
            writer.WriteValueSafe(target);
            writer.WriteNetworkSerializable(message);
            unnamedMessageBroker.SendMessageToOthers(MessageType, writer, NetworkDelivery.ReliableSequenced);
            Route(source, target, message);
        }

        private void HandleMessage(ulong clientId, FastBufferReader reader) {
            reader.ReadValueSafe(out Entity source);
            reader.ReadValueSafe(out Entity target);
            reader.ReadNetworkSerializable(out HitMessage message);
            if (target != null) {
                Route(source, target, message);
            } else {
                this.LogWarning($"Received hit message for non-existent target.");
            }
        }
    }
}
