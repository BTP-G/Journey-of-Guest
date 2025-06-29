using JoG.ObjectPool;
using UnityEngine;

namespace JoG.Projectiles.Pools {

    [CreateAssetMenu(fileName = nameof(BulletPool), menuName = nameof(Projectiles) + "/" + nameof(Pools) + "/" + nameof(BulletPool))]
    public class BulletPool : ComponentPool<SimpleProjectileMotor> {
    }
}