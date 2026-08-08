using JoG.Health;
using JoG.Localization;
using MessagePipe;
using System;
using Unity.Netcode;
using VContainer;
using Xoderony.Localization;

namespace JoG.Gameplay.Objectives {

    public class KillObjective : ObjectiveController, IMessageHandler<DeathMessage> {
        public int targetFactionId = Constants.Factions.Enemy;

        public int currentKills;

        public int requiredKills = 1;

        [Inject] internal ISubscriber<DeathMessage> deathSubscriber;

        private IDisposable sub;

        public override void OnNetworkSpawn() {
            base.OnNetworkSpawn();
            sub = deathSubscriber.Subscribe(this);
            UpdateText();
        }

        public override void OnNetworkDespawn() {
            base.OnNetworkDespawn();
            sub.Dispose();
        }

        void IMessageHandler<DeathMessage>.Handle(DeathMessage message) {
            if (message.entity.GetComponent<Faction>().Id == targetFactionId) {
                currentKills++;
                IsComplete = currentKills >= requiredKills;
                UpdateText();
            }
        }

        protected override void OnSynchronize<T>(ref BufferSerializer<T> serializer) {
            base.OnSynchronize(ref serializer);
            serializer.SerializeValue(ref currentKills);
            UpdateText();
        }

        private void UpdateText() {
            labelText.text = Localizer.GetString(L10nKeys.Objective.KillObjective, currentKills, requiredKills);
        }
    }
}
