//using System.Collections.Generic;
//using VContainer;

//namespace Expriverse.Gameplay {

//    public class Source : INetworkSpawnHandler, INetworkDespawnHandler, IComponent {

//        internal static readonly Dictionary<ulong, Source> IdToAttacker = new();

//        private DelegateSlot<OutgoingDamageMessageModifier> _modifier;

//        private DelegateSlot<OutgoingDamageReportHandler> _handler;

//        [Inject]
//        public Entity Entity { get; internal set; }

//        public void Modify(ref HealthChangeMessage message, in Victim victim) {
//            _modifier.Delegate?.Invoke(ref message, victim);
//        }

//        public void HandleReport(in HealthChangeReport report) {
//            _handler.Delegate?.Invoke(report);
//        }

//        void INetworkSpawnHandler.OnSpawn(bool isOwner) {
//            IdToAttacker[Entity.Id] = this;
//        }

//        void INetworkDespawnHandler.OnDespawn(bool isOwner) {
//            IdToAttacker.Remove(Entity.Id);
//        }

//        [Inject]
//        internal void Inject(DelegateHub delegateHub) {
//            _modifier = delegateHub.GetSlot<OutgoingDamageMessageModifier>();
//            _handler = delegateHub.GetSlot<OutgoingDamageReportHandler>();
//        }
//    }
//}
