using EditorAttributes;
using Unity.Netcode;
using UnityEngine;
using VContainer;

namespace Expriverse.Player {

    [DisallowMultipleComponent]
    public class PlayerIdentity : NetworkBehaviour, IPlayerIdentity {
        [Inject] internal PlayerRegistry _playerRegistry;
        [Inject] internal IProfileService _profileService;
        [ReadOnly, SerializeField] private string _playerName;
        public string PlayerName => _playerName;

        public override void OnNetworkSpawn() {
            if (IsOwner) {
                _playerName = _profileService.Nickname;
            }
            _playerRegistry.Register(this);
        }

        public override void OnNetworkDespawn() {
            _playerRegistry.Unregister(this);
        }

        protected override void OnSynchronize<T>(ref BufferSerializer<T> serializer) {
            serializer.SerializeValue(ref _playerName);
        }
    }
}
