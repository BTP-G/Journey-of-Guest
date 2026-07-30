using Xoderony.Logging;
using Xoderony;
using JoG.Networking;
using MessagePipe;
using System;
using Unity.Collections;
using Unity.Netcode;
using VContainer;
using VContainer.Unity;

namespace JoG.Health {

    public sealed class HealthChangeRouter : IStartable, IDisposable {

        private const byte MessageType = 2;

        [Inject]
        internal IPublisher<HealthChangeReport> reporter;

        [Inject]
        internal UnnamedMessageBroker unnamedMessageBroker;

        public void Start() {
            unnamedMessageBroker.RegisterMessageHandler(MessageType, HandleMessage);
        }

        public void Dispose() {
            unnamedMessageBroker.UnregisterMessageHandler(MessageType, HandleMessage);
        }

        public bool CanDamage(Entity source, Entity target) {
            if (source == null) {
                return true;
            }

            var sourceFaction = source.GetComponent<Faction>();
            var targetFaction = target.GetComponent<Faction>();
            return sourceFaction.IsHostileTo(targetFaction);
        }

        public bool CanHeal(Entity source, Entity target) {
            if (source == null) {
                return false;
            }

            var sourceFaction = source.GetComponent<Faction>();
            var targetFaction = target.GetComponent<Faction>();
            return sourceFaction.IsAlliedWith(targetFaction);
        }

        public void Route(Entity source, Entity target, ref HealthChangeMessage message) {
            if (message.Value < 0) {
                RouteDamage(source, target, ref message);
            } else if (message.Value > 0) {
                RouteHeal(source, target, ref message);
            }
        }

        public void Broadcast(Entity source, Entity target, ref HealthChangeMessage message) {
            using var writer = new FastBufferWriter(64, Allocator.Temp);
            writer.WriteValueSafe(source);
            writer.WriteValueSafe(target);
            writer.WriteNetworkSerializable(message);
            unnamedMessageBroker.SendMessageToOthers(MessageType, writer, NetworkDelivery.ReliableSequenced);
            Route(source, target, ref message);
        }

        private void RouteDamage(Entity source, Entity target, ref HealthChangeMessage message) {
            var outgoingModifier = source?.GetComponent<IDelegateDispatcher<OutgoingDamageMessageModifier>>();
            var incomingModifier = target.GetComponent<IDelegateDispatcher<IncomingDamageMessageModifier>>();
            outgoingModifier?.Handlers?.Invoke(ref message, target);
            incomingModifier.Handlers?.Invoke(ref message, source);
            var resolver = target.GetComponent<IHealthChangeResolver>();
            var report = resolver.Resolve(source, ref message);
            var incomingReportHandlers = target.GetComponent<IDelegateDispatcher<IncomingDamageReportHandler>>();
            var outgoingReportHandlers = source?.GetComponent<IDelegateDispatcher<OutgoingDamageReportHandler>>();
            incomingReportHandlers.Handlers?.Invoke(report);
            outgoingReportHandlers?.Handlers?.Invoke(report);
            reporter.Publish(report);
        }

        private void RouteHeal(Entity source, Entity target, ref HealthChangeMessage message) {
            var outgoingModifier = source?.GetComponent<IDelegateDispatcher<OutgoingHealMessageModifier>>();
            var incomingModifier = target.GetComponent<IDelegateDispatcher<IncomingHealMessageModifier>>();
            outgoingModifier?.Handlers?.Invoke(ref message, target);
            incomingModifier.Handlers?.Invoke(ref message, source);
            var resolver = target.GetComponent<IHealthChangeResolver>();
            var report = resolver.Resolve(source, ref message);
            var incomingReportHandlers = target.GetComponent<IDelegateDispatcher<IncomingHealReportHandler>>();
            var outgoingReportHandlers = source?.GetComponent<IDelegateDispatcher<OutgoingHealReportHandler>>();
            incomingReportHandlers.Handlers?.Invoke(report);
            outgoingReportHandlers?.Handlers?.Invoke(report);
            reporter.Publish(report);
        }

        private void HandleMessage(ulong clientId, FastBufferReader reader) {
            reader.ReadValueSafe(out Entity source);
            reader.ReadValueSafe(out Entity target);
            reader.ReadNetworkSerializable(out HealthChangeMessage message);
            if (target != null) {
                Route(source, target, ref message);
            } else {
                this.LogWarning($"Received health change message for non-existent target.");
            }
        }

    }

}
