using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using VContainer;

namespace JoG.UI.Managers {

    public class PauseMenuManager : MonoBehaviour {
        [SerializeField] private InputActionReference _pauseInput;
        [Inject] private NetworkManager _networkManager;
        public static PauseMenuManager Instance { get; private set; }
        [field: SerializeField] public UnityEvent<bool> OnIsPausedChanged { get; private set; } = new UnityEvent<bool>();
        public bool IsPaused { get; private set; }

        public void Pause() {
            IsPaused = true;
            CursorManager.Instance.RequestShowCursor();
            PlayerCharacterInputer.Instance.ReleaseEnableInput();
            OnIsPausedChanged?.Invoke(true);
        }

        public void Resume() {
            IsPaused = false;
            CursorManager.Instance.ReleaseShowCursor();
            PlayerCharacterInputer.Instance.RequestEnableInput();
            OnIsPausedChanged?.Invoke(false);
        }

        public void StopConnection() {
            PopupManager.PopupConfirm(confirmAction: () => {
                _networkManager.Shutdown();
            });
        }

        public void QuitGame() {
            PopupManager.PopupConfirm(confirmAction: () => {
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
        }

        private void OnEnable() {
            _pauseInput.action.Enable();
        }

        private void OnDisable() {
            _pauseInput.action.Disable();
        }

        private void OnDestroy() {
            _pauseInput.action.performed -= PausePerformed;
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
    }
}