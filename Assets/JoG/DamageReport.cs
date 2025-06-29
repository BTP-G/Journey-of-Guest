using System;
using UnityEngine;

namespace JoG {

    [Serializable]
    public ref struct DamageReport {
        public uint deltaDamage;
        public ulong flag;
        public Vector3 impulse;
        public Vector3 position;
    }
}