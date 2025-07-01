using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;

namespace JoG.UI.Managers {

    public class MainMenuManager : MonoBehaviour {
        [Inject] private NetworkManager _networkManager;
        [Inject] private IAuthenticationService _authenticationService;

        public void QuitGame() {
            PopupManager.PopupConfirm(confirmAction: () => {
#if UNITY_EDITOR
                _networkManager.Shutdown();
                UnityEditor.EditorApplication.isPlaying = false;
#else
                _networkManager.Shutdown();
				Application.Quit();
#endif
            });
        }

        protected async void Awake() {
            _networkManager.OnClientStarted += LoadLobbyScene;
            if (_authenticationService.IsSignedIn) return;
            await LoadingPanelManager.Loading(_authenticationService.SignInAnonymouslyAsync(), "ÕýÔÚµÇÂ¼...");
        }

        protected void OnDestroy() {
            _networkManager.OnClientStarted -= LoadLobbyScene;
        }

        private void LoadLobbyScene() {
            SceneManager.LoadScene("LobbyScene");
        }
    }
}