using System;
using Unity.Netcode;
using UnityEngine;
using VContainer;

namespace Expriverse.Projectiles {

    /// <summary>命中销毁能力：由射弹类型在命中时被动调用。</summary>
    [Serializable]
    public sealed class ProjectileDespawn : IComponent {
        [Min(1)]
        public int delayFrames = 4;

        [Inject] internal NetworkObject networkObject;

        public void Request() {
            networkObject.DeferDespawn(delayFrames, true);
        }
    }
}
