using JoG.ObjectPool;
using UnityEngine;

namespace JoG.UI.Buff {

    [CreateAssetMenu(fileName = nameof(BuffIconPool), menuName = nameof(UI) + "/" + nameof(Buff) + "/" + nameof(BuffIconPool))]
    public class BuffIconPool : ComponentPool<BuffIcon> {
    }
}