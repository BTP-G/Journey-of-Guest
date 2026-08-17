using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer.Unity;
using Xoderony.Networking;
using Xoderony.Networking.Messaging;
using Xoderony.Networking.Serialization;
using Xoderony.Networking.Transport;

namespace JoG.Networking.P2P {
    /// <summary>JoG 对象变量协议；由 VContainer PlayerLoop 每帧刷新脏变量。</summary>
    public sealed class NetworkVariableModule : IInitializable, ITickable, IDisposable {
        private readonly INetworkMessageManager _messageManager;
        private readonly INetworkObjectManager _objectManager;
        private readonly List<JoGNetworkObject> _spawnedObjects = new List<JoGNetworkObject>();

        public NetworkVariableModule(INetworkMessageManager messageManager, INetworkObjectManager objectManager) {
            _messageManager = messageManager;
            _objectManager = objectManager;
        }

        public void Flush() {
            for (var i = 0; i < _spawnedObjects.Count; i++) {
                Flush(_spawnedObjects[i]);
            }
        }

        void IInitializable.Initialize() {
            _messageManager.RegisterMessage(NetworkObjectMessageType.State, OnStateMessage);
            _objectManager.Spawned += OnObjectSpawned;
            _objectManager.Despawned += OnObjectDespawned;
        }

        void ITickable.Tick() {
            Flush();
        }

        public void Dispose() {
            _messageManager.UnregisterMessage(NetworkObjectMessageType.State, OnStateMessage);
            _objectManager.Spawned -= OnObjectSpawned;
            _objectManager.Despawned -= OnObjectDespawned;
            _spawnedObjects.Clear();
        }

        private void OnObjectSpawned(Xoderony.Networking.NetworkObject networkObject, uint id) {
            if (networkObject is not JoGNetworkObject jogNetworkObject || !jogNetworkObject.IsOwner || jogNetworkObject.VariableCount == 0) {
                return;
            }

            Debug.Assert(!_spawnedObjects.Contains(jogNetworkObject), "Network object is already tracked.");
            _spawnedObjects.Add(jogNetworkObject);
        }

        private void OnObjectDespawned(Xoderony.Networking.NetworkObject networkObject, uint id) {
            if (networkObject is JoGNetworkObject jogNetworkObject) {
                _spawnedObjects.Remove(jogNetworkObject);
            }
        }

        private void OnStateMessage(ulong senderPeerId, BufferReader reader) {
            var id = reader.ReadUInt();
            if (!_objectManager.TryGetSpawned(id, out var networkObject) || networkObject is not JoGNetworkObject jogNetworkObject) {
                return;
            }

            if (jogNetworkObject.OwnerPeerId != senderPeerId) {
                Debug.Assert(false, "Only the current owner can send network variable state.");
                return;
            }

            var index = reader.ReadByte();
            jogNetworkObject.DeserializeVariable(index, ref reader);
        }

        private void Flush(JoGNetworkObject networkObject) {
            var firstDirtyIndex = 0;
            while (firstDirtyIndex < networkObject.VariableCount && !networkObject.GetVariable(firstDirtyIndex).IsDirty) {
                firstDirtyIndex++;
            }

            if (firstDirtyIndex == networkObject.VariableCount) {
                return;
            }

            Span<byte> buffer = stackalloc byte[NetworkMessageLimits.MessageCapacity];
            for (var i = firstDirtyIndex; i < networkObject.VariableCount; i++) {
                var variable = networkObject.GetVariable(i);
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
    }
}
