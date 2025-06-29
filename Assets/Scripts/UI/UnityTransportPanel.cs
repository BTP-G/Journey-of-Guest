using EditorAttributes;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using VContainer;

namespace JoG.UI {

    public class UnityTransportPanel : MonoBehaviour {
        [Inject] private NetworkManager _networkManager;
        [Inject] private UnityTransport _transport;
        [SerializeField, Required] private TMP_InputField _ipInputField;
        [SerializeField, Required] private TMP_InputField _portInputField;

        public void SetIpAddress(string address) => _transport.ConnectionData.Address = address;

        public void SetPortByString(string portString) {
            if (ushort.TryParse(portString, out var port)) {
                SetPort(port);
            }
        }

        public void SetPort(ushort port) => _transport.ConnectionData.Port = port;

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