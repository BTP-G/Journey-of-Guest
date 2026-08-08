using System;
using System.Runtime.CompilerServices;

namespace Xoderony.Unity {

    public static class UpdateLoopExtensions {

        /// <summary><inheritdoc cref="PreUpdateLoop{TSystem}.Register(Action)"/></summary>
        /// <returns><inheritdoc cref="PreUpdateLoop{TSystem}.Register(Action)"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool RegisterPre<TSystem>(this Action callback) where TSystem : struct {
            return PreUpdateLoop<TSystem>.Register(callback);
        }

        /// <summary><inheritdoc cref="PreUpdateLoop{TSystem}.Unregister(Action)"/></summary>
        /// <returns><inheritdoc cref="PreUpdateLoop{TSystem}.Unregister(Action)"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool UnregisterPre<TSystem>(this Action callback) where TSystem : struct {
            return PreUpdateLoop<TSystem>.Unregister(callback);
        }

        /// <summary><inheritdoc cref="PostUpdateLoop{TSystem}.Register(Action)"/></summary>
        /// <returns><inheritdoc cref="PostUpdateLoop{TSystem}.Register(Action)"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool RegisterPost<TSystem>(this Action callback) where TSystem : struct {
            return PostUpdateLoop<TSystem>.Register(callback);
        }

        /// <summary><inheritdoc cref="PostUpdateLoop{TSystem}.Unregister(Action)"/></summary>
        /// <returns><inheritdoc cref="PostUpdateLoop{TSystem}.Unregister(Action)"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool UnregisterPost<TSystem>(this Action callback) where TSystem : struct {
            return PostUpdateLoop<TSystem>.Unregister(callback);
        }
    }
}