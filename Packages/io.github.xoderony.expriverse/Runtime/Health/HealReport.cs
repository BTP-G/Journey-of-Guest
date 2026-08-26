//using System;
//using System.Runtime.CompilerServices;

//namespace Expriverse.Health {

//    [Serializable]
//    public struct HealReport {
//        public Healer healer;
//        public Target target;
//        public ulong _flags;
//        public int deltaHeal;

//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        public readonly bool HasFlag(ulong flag) => (_flags & flag) != 0;

//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        public readonly bool HasFlags(ulong _flags) => (this._flags & _flags) == _flags;
//    }
//}
