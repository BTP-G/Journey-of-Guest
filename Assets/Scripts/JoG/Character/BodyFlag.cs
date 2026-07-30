using System;

namespace JoG.Character {

    [Flags]
    public enum BodyFlag : ulong {
        Burnable = 1 << 4,
        Bleedable = 1 << 5,
        Shockable = 1 << 6,
        Poisonable = 1 << 7,
        Curseable = 1 << 8,
        Frostable = 1 << 9,
    }
}
