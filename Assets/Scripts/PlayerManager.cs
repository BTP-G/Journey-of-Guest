using System;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace JoG {

    public class PlayerManager : NetworkBehaviour {
        public NetworkObject networkPlayerPrefab;
        public NetworkObject characterPrefab;
        [Inject] private IAuthenticationService _authenticationService;
        private PlayerCharacterMaster _playerCharacterMaster;
        [SerializeField] private GameObject[] _autoInjectObjects = Array.Empty<GameObject>();

        public void SpawnCharacter() {
            if (_playerCharacterMaster == null) return;
            transform.GetPositionAndRotation(out var position, out var rotation);
            var nob = NetworkManager.SpawnManager.InstantiateAndSpawn(
                characterPrefab,
                destroyWithScene: true,
                isPlayerObject: true,
                position: position,
                rotation: rotation
            );
            if (!nob.TrySetParent(_playerCharacterMaster.NetworkObject, true)) {
                nob.Despawn();
            }
        }

        protected override void OnNetworkSessionSynchronized() {
            var playerName = _authenticationService.PlayerName;
            if (PlayerCharacterMaster.FindPlayer(playerName, out _playerCharacterMaster)) {
                _playerCharacterMaster.NetworkObject.ChangeOwnership(NetworkManager.LocalClientId);
            } else {
                _playerCharacterMaster = NetworkManager.SpawnManager.InstantiateAndSpawn(
                    networkPlayerPrefab,
                    NetworkManager.LocalClientId,
                    false,
                    true).GetComponent<PlayerCharacterMaster>();
                _playerCharacterMaster.PlayerName = playerName;
            }
            var resolver = _playerCharacterMaster.GetComponent<LifetimeScope>().Container;
            foreach (var obj in new ReadOnlySpan<GameObject>(_autoInjectObjects)) {
                resolver.InjectGameObject(obj);
            }
        }
    }
}