using System;
using System.Collections.Generic;
using UnityEngine.Assertions;
using UnityEngine.PlayerLoop;
using VContainer.Unity;
using Xoderony.Networking;
using Xoderony.Networking.Messaging;
using Xoderony.Networking.Serialization;
using Xoderony.Networking.Transport;
using Xoderony.Unity;

namespace JoG.Networking.P2P {
    /// <summary>JoG 对象变量协议；每两个固定步在末尾刷新一次脏变量。</summary>
    public sealed class NetworkVariableModule : IInitializable, INetworkVariableScheduler, IDisposable {
        private readonly INetworkSession _session;
        private readonly INetworkMessageManager _messageManager;
        private readonly INetworkObjectManager _objectManager;
        private readonly HashSet<JoGNetworkObject> _dirtyObjects = new HashSet<JoGNetworkObject>();

        private bool _skipNextFixedTick;

        public NetworkVariableModule(INetworkSession session, INetworkMessageManager messageManager, INetworkObjectManager objectManager) {
            _session = session;
            _messageManager = messageManager;
            _objectManager = objectManager;
        }

        void IInitializable.Initialize() {
            _messageManager.RegisterHandler(NetworkObjectMessageType.State, OnStateMessage);
            _objectManager.Spawned += OnObjectSpawned;
            _objectManager.Despawned += OnObjectDespawned;
            _objectManager.OwnerChanged += OnObjectOwnerChanged;
            PostUpdateLoop<FixedUpdate.ScriptRunDelayedFixedFrameRate>.Register(OnPostFixedUpdate);
        }

        public void Dispose() {
            _messageManager.UnregisterHandler(NetworkObjectMessageType.State, OnStateMessage);
            _objectManager.Spawned -= OnObjectSpawned;
            _objectManager.Despawned -= OnObjectDespawned;
            _objectManager.OwnerChanged -= OnObjectOwnerChanged;
            PostUpdateLoop<FixedUpdate.ScriptRunDelayedFixedFrameRate>.Unregister(OnPostFixedUpdate);
            _dirtyObjects.Clear();
        }

        public void Schedule(JoGNetworkObject networkObject) {
            if (networkObject.OwnerPeerId == _session.LocalPeerId) {
                _dirtyObjects.Add(networkObject);
            }
        }

        private void OnPostFixedUpdate() {
            if (_skipNextFixedTick = !_skipNextFixedTick) {
                return;
            }
            if (_dirtyObjects.Count == 0) {
                return;
            }

            Span<byte> buffer = stackalloc byte[NetworkMessageLimits.MessageCapacity];
            foreach (var networkObject in _dirtyObjects) {
                var variables = networkObject.NetworkVariables;
                for (var i = 0; i < variables.Length; i++) {
                    var variable = variables[i];
                    if (!variable.IsDirty) {
                        continue;
                    }

                    var writer = new BufferWriter(buffer);
                    writer.WriteByte(NetworkObjectMessageType.State);
                    writer.WriteUInt(networkObject.Id);
                    writer.WriteByte((byte)i);
                    variable.Serialize(ref writer);
                    _messageManager.SendToOthers(writer.Written, NetworkDelivery.Reliable);
                }
            }

            _dirtyObjects.Clear();
        }

        private void OnObjectSpawned(NetworkObject networkObject) {
            if (networkObject is not JoGNetworkObject jogNetworkObject || jogNetworkObject.NetworkVariables.IsEmpty) {
                return;
            }

            Schedule(jogNetworkObject);
        }

        private void OnObjectDespawned(NetworkObject networkObject) {
            if (networkObject is JoGNetworkObject jogNetworkObject) {
                _dirtyObjects.Remove(jogNetworkObject);
            }
        }

        private void OnObjectOwnerChanged(NetworkObject networkObject, ulong previousOwnerPeerId, ulong newOwnerPeerId) {
            if (networkObject is not JoGNetworkObject jogNetworkObject || jogNetworkObject.NetworkVariables.IsEmpty) {
                return;
            }

            _dirtyObjects.Remove(jogNetworkObject);
            Schedule(jogNetworkObject);
        }

        private void OnStateMessage(ulong senderPeerId, BufferReader reader) {
            var id = reader.ReadUInt();
            if (!_objectManager.TryGetSpawned(id, out var networkObject) || networkObject is not JoGNetworkObject jogNetworkObject) {
                return;
            }

            var index = reader.ReadByte();
            var variables = jogNetworkObject.NetworkVariables;
            Assert.IsTrue(index < variables.Length, "State variable index is out of range.");
            variables[index].Deserialize(ref reader);
        }
    }
}
