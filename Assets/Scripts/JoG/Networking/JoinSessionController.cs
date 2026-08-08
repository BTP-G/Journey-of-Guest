using Cysharp.Threading.Tasks;
using JoG.Localization;
using JoG.UI.Popup;
using System;
using System.Text.RegularExpressions;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using Xoderony.Extensions;
using Xoderony.Localization;
using Xoderony.Logging;

namespace JoG.Networking {

    internal class JoinSessionController : MonoBehaviour {
        [Inject] internal ISessionService _sessionService;
        [Inject] internal PopupManager _popupManager;
        [Inject] internal NetworkManager _networkManager;
        [SerializeField] private Button _joinButton;
        [SerializeField] private TMP_InputField _sessionCodeInputField;
        [SerializeField] private TMP_InputField _passwordInputField;
        private IDisposable _loader;

        protected void Awake() {
            _joinButton.onClick.AddListener(Join);
        }

        protected void OnEnable() {
            _networkManager.OnConnectionEvent += OnConnectionEvent;
            var clipboard = GUIUtility.systemCopyBuffer;
            if (clipboard.IsNullOrWhiteSpace() || !Regex.IsMatch(clipboard, @"^[A-Z0-9]{6}$")) {
                return;
            }
            _sessionCodeInputField.text = clipboard;
        }

        protected void OnDisable() {
            _networkManager.OnConnectionEvent -= OnConnectionEvent;
            _loader?.Dispose();
        }

        private void OnConnectionEvent(NetworkManager manager, ConnectionEventData data) {
            manager.Log(data.EventType.ToString());
            if (data.EventType == ConnectionEvent.ClientConnected) {
                _sessionService.LeaveSessionAsync().Forget();
            }
            _loader?.Dispose();
            _loader = null;
        }

        private async void Join() {
            _loader = _popupManager.PopupLoader();
            try {
                var sessionCode = _sessionCodeInputField.text;
                var password = _passwordInputField.text;
                await _sessionService.JoinSessionByCodeAsync(sessionCode, password);
            } catch (Exception e) {
                _loader?.Dispose();
                _loader = null;
                this.LogException(e);
                var error = Localizer.GetString(L10nKeys.Session.Join.Failed, e.Message);
                _popupManager.PopupMessage(error, MessageLevel.Error);
            }
        }
    }
}
