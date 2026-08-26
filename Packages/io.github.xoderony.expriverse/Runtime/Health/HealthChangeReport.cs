using System.Runtime.CompilerServices;
using UnityEngine;

namespace Expriverse.Health {

    public struct HealthChangeReport {

        public Entity source;

        public Entity target;

        public HealthChangeFlag flags;

        public Color32 color;

        public int value;

        public int deltaValue;

        public Vector3 position;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool HasFlag(HealthChangeFlag flag) {
            return (flags & flag) != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool HasFlags(HealthChangeFlag flags) {
            return (this.flags & flags) == flags;
        }
    }
}
