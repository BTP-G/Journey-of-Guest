using System.Runtime.CompilerServices;

namespace Xoderony.GameplayEffects {

    public abstract class GameplayEffectData {

        public bool ReadOnly { get; internal set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Validate() {
            OnValidate();
        }

        protected virtual void OnValidate() { }
    }
}
