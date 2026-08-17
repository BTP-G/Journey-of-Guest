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
    /// 提供在指定 PlayerLoop 系统（如 <see cref="Update.ScriptRunBehaviourUpdate"/>）之后执行自定义逻辑的能力。 通过泛型参数
    /// <typeparamref name="TSystem"/> 指定目标系统，自动将其 PostUpdate 阶段注入到 Unity PlayerLoop 中。
    /// </summary>
    /// <typeparam name="TSystem">目标 PlayerLoop 系统类型，必须是 struct 且嵌套在某个顶层阶段中（如 <see cref="Update.ScriptRunBehaviourUpdate"/>）。</typeparam>
    public static class PostUpdateLoop<TSystem> where TSystem : struct {
        private static readonly HashSet<Action> _callbacks = new();
        private static readonly ArrayPool<Action> _arrayPool = ArrayPool<Action>.Shared;

        static PostUpdateLoop() {
            var root = PlayerLoopManager.GetCurrentPlayerLoop();
            var systemType = typeof(TSystem);
            var declaringType = systemType.DeclaringType;
            if (declaringType == null) {
                Debug.LogError($"[PostUpdateLoop<{systemType.Name}>] TSystem must be a nested struct (e.g., inside UnityEngine.PlayerLoop.Update). DeclaringType is null.");
                return;
            }
            foreach (ref var system in root.subSystemList.AsSpan()) {
                if (system.type == declaringType) {
                    var newSize = system.subSystemList.Length + 1;
                    var newSubSystems = new List<PlayerLoopSystem>(newSize);
                    foreach (ref var subSystem in system.subSystemList.AsSpan()) {
                        newSubSystems.Add(subSystem);
                        if (subSystem.type == systemType) {
                            newSubSystems.Add(new PlayerLoopSystem {
                                type = typeof(PostUpdate),
                                updateDelegate = Update,
                            });
                        }
                    }
                    system.subSystemList = newSubSystems.ToArray();
                    PlayerLoopManager.SetPlayerLoop(root);
                    Debug.Log($"[PostUpdateLoop<{systemType.Name}>] Successfully injected PostUpdate stage after {systemType.Name} in the PlayerLoop.");
                    return;
                }
            }
            Debug.LogError($"[PostUpdateLoop<{systemType.Name}>] Failed to locate parent stage {declaringType} in the current PlayerLoop. Injection aborted.");
        }

        /// <summary>注册一个回调方法。该方法将在 <typeparamref name="TSystem"/> 执行后被调用。</summary>
        /// <returns>若成功注册（即之前未注册），返回 true；否则返回 false。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Register(Action callback) {
            return _callbacks.Add(callback);
        }

        /// <summary>注销一个已注册的回调。</summary>
        /// <returns>若成功注销（即之前已注册），返回 true；否则返回 false。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Unregister(Action callback) {
            return _callbacks.Remove(callback);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Update() {
            var count = _callbacks.Count;
            if (count == 0) {
                return;
            }

            var array = _arrayPool.Rent(count);
            try {
                _callbacks.CopyTo(array, 0, count);
                foreach (var callback in new ReadOnlySpan<Action>(array, 0, count)) {
                    callback();
                }
            } finally {
                _arrayPool.Return(array);
            }
        }

        private struct PostUpdate { }
    }
}
