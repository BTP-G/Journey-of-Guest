using GuestUnion.ObjectPool.Generic;
using System;
using System.Reflection;
using static JoG.BuffSystem.BuffPool;

namespace JoG.BuffSystem {

    public static class BuffRegistrar {
        private static readonly Type buffBaseType = typeof(IBuff);
        private static readonly BindingFlags buffIndexFieldBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.SetField | BindingFlags.GetField;

        public static void Register<T>() where T : BuffBase<T>, new() {
            Register(typeof(T));
            SortAllBuffs();
            UpdateAllBuffsIndex();
        }

        /// <summary>注册程序集内的所有Buff，重复的Buff将会产生覆盖</summary>
        /// <param name="assembly">目标程序集</param>
        public static void Register(Assembly assembly) {
            foreach (var type in assembly.GetTypes()) {
                Register(type);
            }
            SortAllBuffs();
            UpdateAllBuffsIndex();
        }

        public static void UnRegister<T>() where T : BuffBase<T>, new() {
            UnRegister(BuffBase<T>.index);
            UpdateAllBuffsIndex();
        }

        /// <summary>注销程序集内的所有Buff</summary>
        /// <param name="assembly">目标程序集</param>
        public static void UnRegister(Assembly assembly) {
            foreach (var type in assembly.GetTypes()) {
                if (type.IsAbstract || !buffBaseType.IsAssignableFrom(type)) {
                    continue;
                }
                var index = (ushort)GetBuffIndexFieldByType(type).GetValue(null);
                UnRegister(index);
            }
            UpdateAllBuffsIndex();
        }

        private static void Register(Type buffType) {
            if (buffType.IsAbstract || !buffBaseType.IsAssignableFrom(buffType)) {
                return;
            }
            var index = (ushort)GetBuffIndexFieldByType(buffType).GetValue(null);
            if (index < buffTypes.Count) {
                UnRegister(index);
            }
            buffTypes.Add(buffType);
            buffPools.Add(StackPool<IBuff>.shared.Rent());
        }

        private static void UnRegister(ushort index) {
            if (index >= buffTypes.Count) return;
            GetBuffIndexFieldByType(buffTypes[index]).SetValue(null, ushort.MaxValue);
            buffTypes.RemoveAt(index);
            StackPool<IBuff>.shared.Return(buffPools[index]);
            buffPools.RemoveAt(index);
        }

        private static void SortAllBuffs() {
            buffTypes.Sort((a, b) => string.CompareOrdinal(a.Name, b.Name));
            foreach (var pool in buffPools) {
                pool.Clear();
            }
        }

        private static void UpdateAllBuffsIndex() {
            for (ushort i = 0; i < buffTypes.Count; ++i) {
                GetBuffIndexFieldByType(buffTypes[i]).SetValue(null, i);
            }
        }

        private static FieldInfo GetBuffIndexFieldByType(Type buffType) {
            var buffRootType = buffType;
            var rootType = typeof(object);
            while (buffRootType.BaseType != rootType) {
                buffRootType = buffRootType.BaseType;
            }
            return buffRootType.GetField("index", buffIndexFieldBindingFlags);
        }
    }
}