using Expriverse.Character;
using Expriverse.Health;
using Expriverse.Localization;
using MessagePipe;
using System;
using Unity.Netcode;
using VContainer;
using Xoderony.Localization;

namespace Expriverse.Gameplay.Objectives {

    public class KillBossObjective : ObjectiveController, IMessageHandler<DeathMessage> {

        [LocalizationKey(@"^character\..*\.name$")]
        public string targetNameKey;

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
            if (message.entity.GetComponent<CharacterNameplate>().nameKey == targetNameKey) {
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
            labelText.text = Localizer.GetString(L10nKeys.Objective.KillBossObjective, currentKills, requiredKills, Localizer.GetString(targetNameKey));
        }
    }
}
