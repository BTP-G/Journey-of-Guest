using Xoderony.Movement;
using System;
using UnityEngine;
using VContainer;

namespace JoG.Character.Components {

    [Serializable]
    public sealed class CharacterMotorNetworkController : IComponent, INetworkSpawnHandler, INetworkDespawnHandler, INetworkOwnershipChangeHandler {

        [Inject] internal CharacterMotor motor;

        void INetworkSpawnHandler.OnSpawn(bool isOwner) {
            ResetTransform();
        }

        void INetworkDespawnHandler.OnDespawn(bool isOwner) {
            ResetTransform();
        }

        void INetworkOwnershipChangeHandler.OnGainedOwnership(bool isCurrentOwner) {
            motor.enabled = isCurrentOwner;
        }

        void INetworkOwnershipChangeHandler.OnLostOwnership(bool isPreviousOwner) {
            motor.enabled = false;
        }

        private void ResetTransform() {
            motor.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        }

    }

}
