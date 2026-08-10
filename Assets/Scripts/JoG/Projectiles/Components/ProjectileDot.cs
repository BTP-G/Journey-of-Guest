using JoG.Character;
using JoG.GameplayEffects;
using System;
using UnityEngine;
using Xoderony.Numerics;
using Xoderony.YooAsset;

namespace JoG.Projectiles {

    /// <summary>命中附加 DoT 能力：由射弹类型在命中时被动调用，销毁时卸载定义引用。</summary>
    [Serializable]
    public sealed class ProjectileDot : IComponent, INetworkDespawnHandler {
        public YooAssetReference<PeriodicHealthChangeDefinition> periodicHealthChangeDefinition;

        [Min(1)]
        public int tickCount = 1;

        [Min(0)]
        public Q16 damageMultiplierPercent = Q16.One;

        private PeriodicHealthChangeDefinition _definition;

        public void Apply(Entity attacker, Entity victim, float damage) {
            if (victim is not CharacterEntity character || (attacker != null && !attacker.HasAuthority)) {
                return;
            }
            var definition = GetDefinition();
            if (definition == null) {
                return;
            }
            var tickValue = -damageMultiplierPercent.Multiply((int)damage);
            if (tickValue == 0) {
                return;
            }
            character.PeriodicHealthChanges.AddEffectRpc(definition, attacker, tickCount, tickValue);
        }

        void INetworkDespawnHandler.OnDespawn(bool isOwner) {
            periodicHealthChangeDefinition.Unload();
            _definition = null;
        }

        private PeriodicHealthChangeDefinition GetDefinition() {
            if (_definition == null) {
                periodicHealthChangeDefinition.Load();
                _definition = periodicHealthChangeDefinition.AssetObject;
            }
            return _definition;
        }
    }
}
