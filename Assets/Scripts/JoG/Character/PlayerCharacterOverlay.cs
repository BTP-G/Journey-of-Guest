using JoG.Health;
using JoG.Player;
using JoG.UI.Buff;
using JoG.UI.Health;
using Unity.Netcode;
using UnityEngine;
using VContainer;

namespace JoG.Character {

    [DisallowMultipleComponent]
    [RequireComponent(typeof(Canvas))]
    public class PlayerCharacterOverlay : MonoBehaviour, IComponent, INetworkOwnershipChangeHandler, INetworkSynchronizeHandler {
        [Inject] internal HealthComponent _health;
        [Inject] internal CharacterEffects _effects;
        [Inject] internal CharacterNameplate _nameplate;
        [Inject] internal CharacterEntity _entity;
        [Inject] internal IProfileService _profileService;
        [SerializeField] private ScreenHealthBar _healthBar;
        [SerializeField] private ScreenBuffBar _buffBar;
        private Canvas _canvas;

        void INetworkOwnershipChangeHandler.OnGainedOwnership(bool isNewOwner) {
            _nameplate.gameObject.SetActive(!isNewOwner);
            _canvas.enabled = isNewOwner;
        }

        void INetworkSynchronizeHandler.OnSynchronize<T>(ref BufferSerializer<T> serializer) {
            var name = _profileService.Nickname;
            serializer.SerializeValue(ref name);
            _nameplate.CharacterName = name;
            _nameplate.nameKey = name;
        }

        void INetworkOwnershipChangeHandler.OnLostOwnership(bool isPreviousOwner) {
            _nameplate.gameObject.SetActive(false);
            _canvas.enabled = false;
        }

        protected void Awake() {
            _canvas = GetComponent<Canvas>();
        }

        protected void LateUpdate() {
            _healthBar.UpdateView(_health.Current, _health.Max);
            if ((Time.frameCount & 0b11) == 0b11) {
                _buffBar.UpdateView(_effects);
            }
        }
    }
}
