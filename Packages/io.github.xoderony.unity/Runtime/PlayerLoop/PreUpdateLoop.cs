using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
using PlayerLoopManager = UnityEngine.LowLevel.PlayerLoop;

namespace Xoderony.Unity {

    /// <summary>
    /// 提供在指定 PlayerLoop 系统（如 <see cref="Update.ScriptRunBehaviourUpdate"/>）之前执行自定义逻辑的能力。 通过泛型参数
    /// <typeparamref name="TSystem"/> 指定目标系统，自动将其 PreUpdate 阶段注入到 Unity PlayerLoop 中。
    /// </summary>
    /// <typeparam name="TSystem">目标 PlayerLoop 系统类型，必须是 struct 且嵌套在某个顶层阶段中（如 <see cref="Update.ScriptRunBehaviourUpdate"/>）。</typeparam>
    public static class PreUpdateLoop<TSystem> where TSystem : struct {
        private static readonly HashSet<Action> _callback = new();
        private static readonly ArrayPool<Action> _arrayPool = ArrayPool<Action>.Shared;

        static PreUpdateLoop() {
            var root = PlayerLoopManager.GetCurrentPlayerLoop();
            var systemType = typeof(TSystem);
            var declaringType = systemType.DeclaringType;
            if (declaringType == null) {
                Debug.LogError($"[PreUpdateLoop<{systemType.Name}>] TSystem must be a nested struct (e.g., inside UnityEngine.PlayerLoop.Update). DeclaringType is null.");
                return;
            }
            foreach (ref var system in root.subSystemList.AsSpan()) {
                if (system.type == declaringType) {
                    var newSize = system.subSystemList.Length + 1;
                    var newSubSystems = new List<PlayerLoopSystem>(newSize);
                    foreach (ref var subSystem in system.subSystemList.AsSpan()) {
                        if (subSystem.type == systemType) {
                            newSubSystems.Add(new PlayerLoopSystem {
                                type = typeof(PreUpdate),
                                updateDelegate = Update,
                            });
                        }
                        newSubSystems.Add(subSystem);
                    }
                    system.subSystemList = newSubSystems.ToArray();
                    PlayerLoopManager.SetPlayerLoop(root);
                    Debug.Log($"[PreUpdateLoop<{systemType.Name}>] Successfully injected PreUpdate stage before {systemType.Name} in the PlayerLoop.");
                    return;
                }
            }
            Debug.LogError($"[PreUpdateLoop<{systemType.Name}>] Failed to locate parent stage {declaringType} in the current PlayerLoop. Injection aborted.");
        }

        /// <summary>注册一个回调方法。该方法将在 <typeparamref name="TSystem"/> 执行前被调用。</summary>
        /// <returns>若成功注册（即之前未注册），返回 true；否则返回 false。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Register(Action callback) {
            return _callback.Add(callback);
        }

        /// <summary>注销一个已注册的回调。</summary>
        /// <returns>若成功注销（即之前已注册），返回 true；否则返回 false。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Unregister(Action callback) {
            return _callback.Remove(callback);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Update() {
            var count = _callback.Count;
            if (count == 0) {
                return;
            }

            var array = _arrayPool.Rent(count);
            try {
                _callback.CopyTo(array, 0, count);
                foreach (var updateable in new ReadOnlySpan<Action>(array, 0, count)) {
                    updateable();
                }
            } finally {
                _arrayPool.Return(array);
            }
        }

        private struct PreUpdate { }
    }
}