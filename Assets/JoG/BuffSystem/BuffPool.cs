using System.Collections.Generic;
using System;

namespace JoG.BuffSystem {
    public static class BuffPool {
        internal static readonly List<Type> buffTypes = new();
        internal static readonly List<Stack<IBuff>> buffPools = new();

        public static ushort Count => (ushort)buffTypes.Count;

        public static IBuff Rent(ushort index) =>
            buffPools[index].TryPop(out var result)
            ? result
            : Activator.CreateInstance(buffTypes[index]) as IBuff;

        public static T Rent<T>() where T : BuffBase<T>, new() =>
            buffPools[BuffBase<T>.index].TryPop(out var result)
            ? result as T
            : new();

        public static void Return(IBuff buff) => buffPools[buff.Index].Push(buff);

    }
}
