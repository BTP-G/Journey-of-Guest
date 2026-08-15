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
        private readonly INetworkManager _networkManager;
        private readonly INetworkObjectEvents _objectEvents;
        private readonly INetworkObjectResolver _objectResolver;
        private readonly List<JoGNetworkObject> _spawnedObjects = new List<JoGNetworkObject>();

        public NetworkVariableModule(INetworkManager networkManager, INetworkObjectEvents objectEvents, INetworkObjectResolver objectResolver) {
            _networkManager = networkManager;
            _objectEvents = objectEvents;
            _objectResolver = objectResolver;
        }

        public void Flush() {
            for (var i = 0; i < _spawnedObjects.Count; i++) {
                Flush(_spawnedObjects[i]);
            }
        }

        void IInitializable.Initialize() {
            _networkManager.Started += OnSessionStarted;
            _networkManager.Stopped += OnSessionStopped;
            _objectEvents.Spawned += OnObjectSpawned;
            _objectEvents.Despawning += OnObjectDespawning;
        }

        void ITickable.Tick() {
            Flush();
        }

        public void Dispose() {
            _networkManager.Started -= OnSessionStarted;
            _networkManager.Stopped -= OnSessionStopped;
            _networkManager.UnregisterMessage(NetworkObjectMessageType.State, OnStateMessage);
            _objectEvents.Spawned -= OnObjectSpawned;
            _objectEvents.Despawning -= OnObjectDespawning;
            _spawnedObjects.Clear();
        }

        private void OnSessionStarted() {
            _networkManager.RegisterMessage(NetworkObjectMessageType.State, OnStateMessage);
        }

        private void OnSessionStopped() {
            _networkManager.UnregisterMessage(NetworkObjectMessageType.State, OnStateMessage);
            _spawnedObjects.Clear();
        }

        private void OnObjectSpawned(Xoderony.Networking.NetworkObject networkObject) {
            if (networkObject is not JoGNetworkObject jogNetworkObject || !jogNetworkObject.IsOwner || jogNetworkObject.VariableCount == 0) {
                return;
            }

            Debug.Assert(!_spawnedObjects.Contains(jogNetworkObject), "Network object is already tracked.");
            _spawnedObjects.Add(jogNetworkObject);
        }

        private void OnObjectDespawning(Xoderony.Networking.NetworkObject networkObject) {
            if (networkObject is JoGNetworkObject jogNetworkObject) {
                _spawnedObjects.Remove(jogNetworkObject);
            }
        }

        private void OnStateMessage(ulong senderPeerId, BufferReader reader) {
            var id = new NetworkObjectId(senderPeerId, reader.ReadUInt());
            if (!_objectResolver.TryGetSpawned(id, out var networkObject) || networkObject is not JoGNetworkObject jogNetworkObject) {
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

            Span<byte> buffer = stackalloc byte[NetworkMessageLimits.PayloadCapacity];
            for (var i = firstDirtyIndex; i < networkObject.VariableCount; i++) {
                var variable = networkObject.GetVariable(i);
                if (!variable.IsDirty) {
                    continue;
                }

                var writer = new BufferWriter(buffer);
                writer.WriteUInt(networkObject.Id.Sequence);
                writer.WriteByte((byte)i);
                variable.Serialize(ref writer);
                _networkManager.SendToOthers(NetworkObjectMessageType.State, writer.Written, NetworkDelivery.Reliable);
            }
        }
    }
}
