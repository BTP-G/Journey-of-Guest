using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine.Assertions;
using VContainer;
using Xoderony.Networking.Serialization;
using Xoderony.ObjectPool.Generic;

namespace JoG.Networking.P2P {
    /// <summary>JoG 对象协议：在 Awake 中固定收集 NV 与 RPC 端点。</summary>
    public class JoGNetworkObject : Xoderony.Networking.NetworkObject {
        private const int MaxVariableCount = byte.MaxValue + 1;
        private const int MaxRpcCount = byte.MaxValue + 1;

        [Inject] internal INetworkRpcSender rpcSender;
        [Inject] internal INetworkVariableSyncScheduler networkVariableScheduler;

        private NetworkVariableBase[] _networkVariables = Array.Empty<NetworkVariableBase>();
        private NetworkRpcBase[] _networkRpcs = Array.Empty<NetworkRpcBase>();

        internal ReadOnlySpan<NetworkVariableBase> NetworkVariables => _networkVariables;

        internal ReadOnlySpan<NetworkRpcBase> NetworkRpcs => _networkRpcs;

        protected virtual void Awake() {
            using (ListPool<NetworkVariableBase>.Rent(out var variables)) {
                CollectNetworkVariables(variables);
                Assert.IsTrue(variables.Count <= MaxVariableCount, "Too many network variables.");
                AssertNoDuplicateNetworkVariables(variables);
                var networkVariables = new NetworkVariableBase[variables.Count];
                for (var i = 0; i < networkVariables.Length; i++) {
                    var variable = variables[i];
                    variable.Bind(this, (byte)i);
                    networkVariables[i] = variable;
                }
                _networkVariables = networkVariables;
            }

            using (ListPool<NetworkRpcBase>.Rent(out var rpcs)) {
                CollectNetworkRpcs(rpcs);
                Assert.IsTrue(rpcs.Count <= MaxRpcCount, "Too many network RPCs.");
                AssertNoDuplicateNetworkRpcs(rpcs);
                var networkRpcs = new NetworkRpcBase[rpcs.Count];
                for (var i = 0; i < networkRpcs.Length; i++) {
                    var rpc = rpcs[i];
                    rpc.Bind(this, (byte)i);
                    networkRpcs[i] = rpc;
                }
                _networkRpcs = networkRpcs;
            }
        }

        /// <summary>派生类重写此方法，以声明顺序收集网络变量。</summary>
        protected virtual void CollectNetworkVariables(List<NetworkVariableBase> variables) {
        }

        /// <summary>派生类重写此方法，以声明顺序收集 RPC 端点。</summary>
        protected virtual void CollectNetworkRpcs(List<NetworkRpcBase> rpcs) {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void SendRpcToOthers(byte index, ReadOnlySpan<byte> payload) {
            rpcSender.SendToOthers(this, index, payload);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void SendRpcToAll(byte index, ReadOnlySpan<byte> payload) {
            rpcSender.SendToAll(this, index, payload);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void SendRpcToOwner(byte index, ReadOnlySpan<byte> payload) {
            rpcSender.SendToOwner(this, index, payload);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void SendRpcToPeer(ulong peerId, byte index, ReadOnlySpan<byte> payload) {
            rpcSender.SendToPeer(this, peerId, index, payload);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void MarkNetworkVariableDirty() {
            networkVariableScheduler.Schedule(this);
        }

        protected override void OnSerializeSnapshot(ref BufferWriter writer) {
            for (var i = 0; i < _networkVariables.Length; i++) {
                _networkVariables[i].Serialize(ref writer);
            }

            base.OnSerializeSnapshot(ref writer);
        }

        protected override void OnDeserializeSnapshot(ref BufferReader reader) {
            for (var i = 0; i < _networkVariables.Length; i++) {
                _networkVariables[i].Deserialize(ref reader);
            }

            base.OnDeserializeSnapshot(ref reader);
        }

        [Conditional("UNITY_ASSERTIONS")]
        private static void AssertNoDuplicateNetworkVariables(List<NetworkVariableBase> variables) {
            for (var i = 0; i < variables.Count; i++) {
                Assert.AreEqual(i, variables.IndexOf(variables[i]), "Network variable is collected more than once.");
            }
        }

        [Conditional("UNITY_ASSERTIONS")]
        private static void AssertNoDuplicateNetworkRpcs(List<NetworkRpcBase> rpcs) {
            for (var i = 0; i < rpcs.Count; i++) {
                Assert.AreEqual(i, rpcs.IndexOf(rpcs[i]), "Network RPC is collected more than once.");
            }
        }
    }
}
