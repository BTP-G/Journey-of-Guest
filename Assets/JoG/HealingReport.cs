using System;

namespace JoG {

    [Serializable]
    public ref struct HealingReport {
        public uint deltaHealing;
        public ulong flag;
    }
}