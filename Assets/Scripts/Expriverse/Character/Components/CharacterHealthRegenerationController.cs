using Expriverse.Health;
using System;
using UnityEngine;
using UnityEngine.PlayerLoop;
using VContainer;
using Xoderony.Unity;

namespace Expriverse.Character.Components {

    [Serializable]
    public sealed class CharacterHealthRegenerationController : IComponent, INetworkSpawnHandler, INetworkDespawnHandler {

        [Inject] internal HealthComponent health;

        [Inject, Key(Constants.Stats.Regen)] internal Stat regen;

        private float _regenBuffer;

        void INetworkSpawnHandler.OnSpawn(bool isOwner) {
            PostUpdateLoop<FixedUpdate.ScriptRunDelayedFixedFrameRate>.Register(OnPostDelayedFixedUpdate);
        }

        void INetworkDespawnHandler.OnDespawn(bool isOwner) {
            PostUpdateLoop<FixedUpdate.ScriptRunDelayedFixedFrameRate>.Unregister(OnPostDelayedFixedUpdate);
            _regenBuffer = 0;
        }

        private void OnPostDelayedFixedUpdate() {
            if (health.IsAlive && health.Current < health.Max) {
                _regenBuffer += Time.fixedDeltaTime * regen.Value;
                var regenAmount = Mathf.FloorToInt(_regenBuffer);
                if (regenAmount == 0) {
                    return;
                }

                health.Current = Math.Min(health.Current + regenAmount, health.Max);
                _regenBuffer -= regenAmount;
            } else {
                _regenBuffer = 0;
            }
        }
    }
}
