using JoG.Combat;
using JoG.Health;
using System;
using UnityEngine;
using VContainer;

namespace JoG.Projectiles {

    /// <summary>区域爆炸能力：由射弹类型在命中或销毁时被动调用。</summary>
    [Serializable]
    public sealed class ProjectileExplosion : IComponent {
        [Min(0)] public float radius = 1f;
        public LayerMask hitLayer = LayerMasks.CharacterPart | LayerMasks.Prop;
        public AnimationCurve falloff = AnimationCurve.Linear(0, 1, 1, 0);
        public HealthChangeFlag damageFlags;

        [Inject] internal CombatDamage combatDamage;

        public void Detonate(Entity attacker, in Vector3 position, float damage) {
            combatDamage.ApplySphere(attacker, position, radius, hitLayer, QueryTriggerInteraction.Collide, damage, damageFlags, falloff, broadcast: true);
        }
    }
}
