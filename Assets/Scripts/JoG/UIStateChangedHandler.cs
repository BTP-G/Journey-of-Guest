using Xoderony.Logging;
using MessagePipe;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using VContainer.Unity;

namespace JoG {

    internal class UIStateChangedHandler : IStartable, IDisposable, IMessageHandler<UIStateChangedMessage> {
        private readonly int[] _layerToCount;
        private readonly HashSet<string> _activeUI = new();
        private IDisposable _disposable;
        [Inject, Key(Constants.InputActionMap.Gameplay)] internal InputActionMap _gameplayMap;
        [Inject, Key(Constants.InputActionMap.Overlay)] internal InputActionMap _overlayMap;
        [Inject, Key(Constants.InputActionMap.Menu)] internal InputActionMap _menuMap;

        public UIStateChangedHandler(ISubscriber<UIStateChangedMessage> subscriber) {
            _disposable = subscriber.Subscribe(this);
            _layerToCount = new int[Enum.GetNames(typeof(UILayer)).Length];
        }

        public void Start() {
            _gameplayMap.Enable();
            _overlayMap.Enable();
            _menuMap.Enable();
            UpdateState();
        }

        public void Dispose() {
            _disposable.Dispose();
            _gameplayMap.Disable();
            _overlayMap.Disable();
            _menuMap.Disable();
            Cursor.lockState = CursorLockMode.None;
        }

        public void Handle(UIStateChangedMessage message) {
            this.Log(message.Name + message.Active);
            if (message.Active) {
                if (_activeUI.Add(message.Name)) {
                    _layerToCount[(int)message.Layer]++;
                    UpdateState();
                } else {
                    this.LogWarning(message.Name + " is already active.");
                }
            } else {
                if (_activeUI.Remove(message.Name)) {
                    _layerToCount[(int)message.Layer]--;
                    UpdateState();
                } else {
                    this.LogWarning(message.Name + " is already inactive.");
                }
            }
        }

        private void UpdateState() {
            if (_layerToCount[(int)UILayer.Menu] > 0) {
                _gameplayMap.Disable();
                _overlayMap.Disable();
                Cursor.lockState = CursorLockMode.None;
                return;
            }
            if (_layerToCount[(int)UILayer.Overlay] > 0) {
                _gameplayMap.Disable();
                _overlayMap.Enable();
                Cursor.lockState = CursorLockMode.Confined;
                return;
            }
            _gameplayMap.Enable();
            _overlayMap.Enable();
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}
