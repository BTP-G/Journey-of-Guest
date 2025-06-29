using EditorAttributes;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JoG {

    [DisallowMultipleComponent]
    public class GameManager : MonoBehaviour {
        public NetworkManager NetworkManager { get; private set; }

        [Button]
        public void LoadLobbySceneServer() {
            NetworkManager.SceneManager.LoadScene("LobbyScene", LoadSceneMode.Single);
        }

        public void StopConnection() {
            NetworkManager.Shutdown();
        }

        //private void Awake() {
        //    NetworkManager = GetComponent<NetworkManager>();
        //    NetworkManager.On += ServerManager_OnServerConnectionState;
        //    NetworkManager.ClientManager.OnClientConnectionState += ClientManager_OnClientConnectionState;
        //}

        //private void ServerManager_OnServerConnectionState(ServerConnectionStateArgs args) {
        //    if (args.ConnectionState is LocalConnectionState.Started) {
        //        LoadLobbySceneServer();
        //    }
        //}

        //private void ClientManager_OnClientConnectionState(ClientConnectionStateArgs args) {
        //    if (args.ConnectionState is LocalConnectionState.Stopped) {
        //        if (USceneManager.GetActiveScene().name is not "MainScene") {
        //            USceneManager.LoadScene("MainScene");
        //        }
        //    }
        //}
    }
}