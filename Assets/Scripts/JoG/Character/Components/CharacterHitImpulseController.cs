using JoG.Health;
using System;
using VContainer;
using Xoderony;
using Xoderony.Extensions;
using Xoderony.Movement;

namespace JoG.Character.Components {

    [Serializable]
    public sealed class CharacterHitImpulseController : IComponent, INetworkSpawnHandler, INetworkDespawnHandler {

        [Inject]
        internal Entity entity;

        [Inject]
        internal CharacterMotor motor;

        [Inject]
        internal IDelegateSubscriber<IncomingHitMessageHandler> incomingHitMessages;

        void INetworkSpawnHandler.OnSpawn(bool isOwner) {
            incomingHitMessages.Subscribe(OnIncomingHit);
        }

        void INetworkDespawnHandler.OnDespawn(bool isOwner) {
            incomingHitMessages.Unsubscribe(OnIncomingHit);
        }

        private void OnIncomingHit(in HitMessage message, Entity source) {
            if (!entity.HasAuthority) {
                return;
            }

            motor.AddImpulse(message.impulse);
            if (motor.ExternalVelocity.Dot(motor.Up) > motor.MaxStepHeight) {
                motor.ForceUngrounded(2);
            }
        }
    }
}
