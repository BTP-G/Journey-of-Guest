using System;
using UnityEngine;
using VContainer;
using Xoderony.Movement;

namespace Expriverse.Character.Components {

    [Serializable]
    public sealed class CharacterMotorNetworkController : IComponent, INetworkSpawnHandler, INetworkDespawnHandler, INetworkAuthorityChangedHandler {

        [Inject] internal CharacterMotor motor;

        void INetworkSpawnHandler.OnSpawn(bool isOwner) {
            ResetTransform();
        }

        void INetworkDespawnHandler.OnDespawn(bool isOwner) {
            ResetTransform();
        }

        void INetworkAuthorityChangedHandler.OnAuthorityChanged(bool hasAuthority) {
            motor.enabled = hasAuthority;
        }

        private void ResetTransform() {
            motor.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        }
    }
}
