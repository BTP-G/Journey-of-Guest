using UnityEngine;
using VContainer;
using Xoderony.Networking;

namespace Expriverse.Networking.P2P {
    /// <summary>编辑器挂载的最小 P2P 验收入口；不参与正式玩法启动。</summary>
    public sealed class P2PValidationSpawner : MonoBehaviour {
        [SerializeField] private P2PValidationPlayerObject _playerPrefab;
        [SerializeField] private P2PValidationPersistentObject _persistentPrefab;

        [Inject] internal INetworkObjectManager _objectManager;
        [Inject] internal INetworkSession _session;

        private P2PValidationPlayerObject _playerInstance;
        private P2PValidationPersistentObject _persistentInstance;

        public void SpawnPlayer() {
            _playerInstance = Spawn(_playerPrefab, 1);
        }

        public void SpawnPersistent() {
            _persistentInstance = Spawn(_persistentPrefab, 100);
        }

        public void SetPlayerValue(int value) {
            if (_playerInstance != null && _playerInstance.IsSpawned && _playerInstance.OwnerPeerId == _session.LocalPeerId) {
                _playerInstance.SetSnapshotValue(value);
            }
        }

        public void SendPlayerBroadcast(int value) {
            if (_playerInstance != null && _playerInstance.IsSpawned) {
                _playerInstance.SendBroadcast(value);
            }
        }

        public void DespawnPlayer() {
            if (Despawn(_playerInstance)) {
                _playerInstance = null;
            }
        }

        public void DespawnPersistent() {
            if (Despawn(_persistentInstance)) {
                _persistentInstance = null;
            }
        }

        private T Spawn<T>(T prefab, int initialValue) where T : P2PValidationNetworkObject {
            if (!_session.IsStarted || prefab == null) {
                return null;
            }

            return (T)_objectManager.Spawn(prefab, instance => ((T)instance).SetSnapshotValue(initialValue));
        }

        private bool Despawn(Xoderony.Networking.NetworkObject instance) {
            if (instance == null || !instance.IsSpawned || instance.OwnerPeerId != _session.LocalPeerId) {
                return false;
            }

            _objectManager.Despawn(instance);
            return true;
        }
    }
}
