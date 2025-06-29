using JoG.ObjectPool;
using UnityEngine;

namespace JoG.Effects.Pools {

    [CreateAssetMenu(fileName = nameof(EffectPool), menuName = nameof(Effects) + "/" + nameof(Pools) + "/" + nameof(EffectPool))]
    public class EffectPool : ComponentPool<EffectController> {
    }
}