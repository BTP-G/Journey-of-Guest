using JoG.Messages;
using JoG.Utility;
using MessagePipe;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using VContainer;

namespace JoG.UI.Managers {

    public class PauseMenuManager : MonoBehaviour {
        [SerializeField] private InputActionReference _pauseInput;
        [SerializeField] private InputActionReference _altInput;
        [Inject] private NetworkManager _networkManager;
        [Inject] private IPublisher<FocusOnUIMessage> _publisher;
        public static PauseMenuManager Instance { get; private set; }
        [field: SerializeField] public UnityEvent<bool> OnIsPausedChanged { get; private set; } = new UnityEvent<bool>();
        public bool IsPaused { get; private set; }

        public void Pause() {
            IsPaused = true;
            Cursor.lockState = CursorLockMode.None;
            CursorManager.lockOnFocus = false;
            OnIsPausedChanged?.Invoke(true);
            _publisher.Publish(new FocusOnUIMessage(true));
        }

        public void Resume() {
            IsPaused = false;
            Cursor.lockState = CursorLockMode.Locked;
            CursorManager.lockOnFocus = true;
            OnIsPausedChanged?.Invoke(false);
            _publisher.Publish(new FocusOnUIMessage(false));
        }

        public void StopConnection() {
            ConfirmPopupManager.Popup(confirmAction: () => {
                _networkManager.Shutdown();
            });
        }

        public void QuitGame() {
            ConfirmPopupManager.Popup(confirmAction: () => {
                _networkManager.OnClientStopped -= OnClientStopped;
                _networkManager.Shutdown();
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
				Application.Quit();
#endif
            });
        }

        private void Awake() {
            Instance = this;
            _networkManager.OnClientStopped += OnClientStopped;
            _pauseInput.action.performed += PausePerformed;
            _altInput.action.started += HandleAltInput;
            _altInput.action.canceled += HandleAltInput;
        }

        private void OnEnable() {
            _pauseInput.action.Enable();
            _altInput.action.Enable();
        }

        private void OnDisable() {
            _pauseInput.action.Disable();
            _altInput.action.Disable();
            Cursor.lockState = CursorLockMode.None;
        }

        private void OnDestroy() {
            _pauseInput.action.performed -= PausePerformed;
            _altInput.action.started -= HandleAltInput;
            _altInput.action.canceled -= HandleAltInput;
            _networkManager.OnClientStopped -= OnClientStopped;
            Instance = null;
        }

        private void OnClientStopped(bool obj) {
            SceneManager.LoadScene("MainScene");
        }

        private void PausePerformed(InputAction.CallbackContext _) {
            if (IsPaused) {
                Resume();
            } else {
                Pause();
            }
        }

        private void HandleAltInput(InputAction.CallbackContext context) {
            if (context.started) {
                Cursor.lockState = CursorLockMode.Confined;
            } else if (context.canceled) {
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
    }
}