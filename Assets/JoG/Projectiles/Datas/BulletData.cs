using JoG.Effects.Pools;
using System;
using UnityEngine;

namespace JoG.Projectiles.Datas {

    [CreateAssetMenu(fileName = nameof(BulletData), menuName = nameof(Projectiles) + "/" + nameof(Datas) + "/" + nameof(BulletData))]
    public class BulletData : ScriptableObject {
        [Range(0, 1)] public float piercingCofficient = 0f;
        [Range(0, float.MaxValue)] public float damage = 1;
        [Range(0, float.MaxValue)] public float mass = 0.01f;
        [Range(0, float.MaxValue)] public float speed = 100f;
        [Tooltip("不影响已激活的子弹")] public float lifetime = 1f;
        public LayerMask hitLayerMask;
        public EffectPool impactEffectPool;
    }
}