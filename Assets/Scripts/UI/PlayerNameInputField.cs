using EditorAttributes;
using TMPro;
using Unity.Services.Authentication;
using UnityEngine;
using VContainer;

namespace JoG {

    public class PlayerNameInputField : MonoBehaviour {
        [Inject] private IAuthenticationService _authenticationService;
        [SerializeField, Required] private TMP_InputField _playerNameInputField;

        public async void SetPlayerName(string name) {
            await _authenticationService.UpdatePlayerNameAsync(name);
            _playerNameInputField.text = _authenticationService.PlayerName;
        }

        private void Awake() {
            _authenticationService.SignedIn += OnSignedIn;
            if (_authenticationService.IsSignedIn) {
                OnSignedIn();
            }
            _playerNameInputField.onEndEdit.AddListener(SetPlayerName);
        }

        private void OnDestroy() {
            _authenticationService.SignedIn -= OnSignedIn;
            _playerNameInputField.onEndEdit.RemoveListener(SetPlayerName);
        }

        private void OnSignedIn() {
            _playerNameInputField.text = _authenticationService.PlayerName;
        }
    }
}