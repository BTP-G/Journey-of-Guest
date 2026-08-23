using Cysharp.Threading.Tasks;
using MessagePipe;
using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using VContainer;
using Xoderony.Logging;

namespace JoG.UI {

    public class IngameMenuController : MonoBehaviour {
        public UnityEvent2 enable = new();
        public UnityEvent2 disable = new();
        [Inject, Key(Constants.InputAction.IngameMenu)] internal InputAction toggleInput;
        [Inject] internal NetworkManager networkManager;
        [Inject] internal IPublisher<UIStateChangedMessage> publisher;
        [Inject] internal SceneTransitionService sceneTransitionService;

        private void Awake() {
            networkManager.OnClientStopped += OnClientStopped;
            toggleInput.performed += OnToggleInput;
        }

        private void OnEnable() {
            publisher.Publish(new(name, UILayer.Menu, true));
            enable.Invoke();
        }

        private void OnDisable() {
            publisher.Publish(new(name, UILayer.Menu, false));
            disable.Invoke();
        }

        private void OnDestroy() {
            toggleInput.performed -= OnToggleInput;
            networkManager.OnClientStopped -= OnClientStopped;
        }

        private void OnApplicationFocus(bool focus) {
            if (focus) {
                return;
            }

            enabled = true;
        }

        private void OnClientStopped(bool obj) {
            sceneTransitionService.LoadMainSceneAsync(destroyCancellationToken).Forget(HandleSceneTransitionException);
        }

        private void HandleSceneTransitionException(Exception exception) {
            if (exception is not OperationCanceledException) {
                this.LogException(exception);
            }
        }

        private void OnToggleInput(InputAction.CallbackContext _) {
            enabled = !enabled;
        }
    }
}
