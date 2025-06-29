using Netcode.Transports.Facepunch;
using Unity.Netcode;
using UnityEngine;
using VContainer;

namespace JoG.UI {

    public class FacepunchTransportController : MonoBehaviour {
        [Inject] private FacepunchTransport _transport;
        [Inject] private NetworkManager _networkManager;

        public void SetTargetSteamIdByString(string steamIdString) {
            if (ulong.TryParse(steamIdString, out var steamId)) {
                SetTargetSteamId(steamId);
            }
        }

        public void SetTargetSteamId(ulong steamId) => _transport.targetSteamId = steamId;

        public void StartHost() {
            _networkManager.NetworkConfig.NetworkTransport = _transport;
            _networkManager.StartHost();
        }

        public void StartServer() {
            _networkManager.NetworkConfig.NetworkTransport = _transport;
            _networkManager.StartServer();
        }

        public void StartClient() {
            _networkManager.NetworkConfig.NetworkTransport = _transport;
            _networkManager.StartClient();
        }
    }
}