using System;
using UnityEngine;

namespace Expriverse.Projectiles {

    /// <summary>穿透能力：命中计数，达到上限后由射弹类型销毁。</summary>
    [Serializable]
    public sealed class ProjectilePenetration : IComponent {
        [Min(0)]
        public int penetrateCount;

        private int _hitCount;

        /// <returns>本次命中后是否继续飞行。</returns>
        public bool RecordHit() {
            return ++_hitCount <= penetrateCount;
        }
    }
}
