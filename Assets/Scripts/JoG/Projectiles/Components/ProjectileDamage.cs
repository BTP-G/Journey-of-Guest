using JoG.Combat;
using JoG.Health;
using System;
using UnityEngine;
using VContainer;

namespace JoG.Projectiles {

    /// <summary>单目标直伤能力：由射弹类型在命中时被动调用。</summary>
    [Serializable]
    public sealed class ProjectileDamage : IComponent {
        public HealthChangeFlag damageFlags;

        [Inject] internal CombatDamage combatDamage;

        public void Apply(Entity attacker, Collider collider, in Vector3 point, float damage) {
            combatDamage.ApplySingle(attacker, collider, point, damage, damageFlags, broadcast: true);
        }
    }
}
