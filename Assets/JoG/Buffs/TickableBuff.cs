using JoG.BuffSystem;
using Unity.Netcode;
using UnityEngine;

namespace JoG.Buffs {

    public abstract class TickableBuff<T> : BuffBase<T>, INetworkUpdateSystem where T : TickableBuff<T>, new() {
        private float nextTickTime;
        public abstract float TickInterval { get; }

        void INetworkUpdateSystem.NetworkUpdate(NetworkUpdateStage updateStage) {
            if (Time.fixedTime < nextTickTime) return;
            OnTick();
            nextTickTime = Time.fixedTime + TickInterval;
        }

        protected override void OnAddedToOwner() {
            nextTickTime = Time.fixedTime + TickInterval;
            this.RegisterNetworkUpdate(NetworkUpdateStage.FixedUpdate);
        }

        protected abstract void OnTick();

        protected override void OnRemoveFromOwner() {
            this.UnregisterNetworkUpdate(NetworkUpdateStage.FixedUpdate);
        }
    }
}