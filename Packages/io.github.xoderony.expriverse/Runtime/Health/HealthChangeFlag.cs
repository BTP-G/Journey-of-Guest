using System;

namespace Expriverse.Health {

    [Flags]
    public enum HealthChangeFlag {
        None = 0,
        Direct = 1 << 0,
        DamageOverTime = 1 << 1,
        Reflect = 1 << 2,
        HolySword = 1 << 3,
        LifeSteal = 1 << 4,
    }
}
