using Cysharp.Threading.Tasks;
using JoG.Localization;
using JoG.Player;
using JoG.UI.Popup;
using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VContainer;
using Xoderony.Localization;
using Xoderony.Logging;

namespace JoG.Networking {

    internal class CreateSessionController : MonoBehaviour {
        [Inject] internal IProfileService _profileService;
        [Inject] internal ISessionService _sessionService;
        [Inject] internal PopupManager _popupManager;
        [Inject] internal NetworkManager _networkManager;
        [SerializeField] private Button _createButton;
        [SerializeField] private TMP_InputField _sessionNameInputField;
        [SerializeField] private TMP_InputField _passwordInputField;
        [SerializeField] private TMP_InputField _maxPlayersInputField;
        [SerializeField] private Toggle _isPrivateToggle;
        private IDisposable _loader;

        protected void Awake() {
            _createButton.onClick.AddListener(Create);
            _sessionNameInputField.text = Localizer.GetString(L10nKeys.Session.Create.DefaultName, _profileService.Nickname);
            _maxPlayersInputField.text = "4";
        }

        private void OnEnable() {
            _networkManager.OnConnectionEvent += OnConnectionEvent;
        }

        private void OnDisable() {
            _networkManager.OnConnectionEvent -= OnConnectionEvent;
        }

        private void OnConnectionEvent(NetworkManager networkManager, ConnectionEventData data) {
            networkManager.Log(data.EventType.ToString());
            if (data.EventType == ConnectionEvent.ClientConnected) {
                networkManager.SceneManager.LoadScene("Demo", LoadSceneMode.Single);
            } else if (data.EventType == ConnectionEvent.ClientConnected) {
                _sessionService.LeaveSessionAsync().Forget();
                var error = Localizer.GetString(L10nKeys.Session.Create.Failed, "error");
                _popupManager.PopupMessage(error, MessageLevel.Error);
            }
            _loader?.Dispose();
            _loader = null;
        }

        private async void Create() {
            _loader = _popupManager.PopupLoader();
            try {
                await _sessionService.CreateSessionAsync(
                     _sessionNameInputField.text,
                     _passwordInputField.text,
                     int.Parse(_maxPlayersInputField.text),
                     _isPrivateToggle.isOn
                );
                var message = Localizer.GetString(L10nKeys.Session.Create.Created, _sessionService.Session.Code);
                GUIUtility.systemCopyBuffer = _sessionService.Session.Code;
                _popupManager.PopupToast(message, MessageLevel.Info, ToastPosition.Top);
            } catch (Exception e) {
                _loader?.Dispose();
                _loader = null;
                this.LogException(e);
                var error = Localizer.GetString(L10nKeys.Session.Create.Failed, e.Message);
                _popupManager.PopupMessage(error, MessageLevel.Error);
            }
        }
    }
}
