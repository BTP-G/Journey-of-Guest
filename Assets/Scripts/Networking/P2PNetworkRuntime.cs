using System;
using VContainer.Unity;
using Xoderony.Logging;
using Xoderony.Networking;

namespace JoG.Networking.P2P {
    /// <summary>RootScope 中启动并持有 P2P 技术验证栈。</summary>
    public sealed class P2PNetworkRuntime : IInitializable, IDisposable {
        private readonly SteamNetworkTransport _transport;

        public bool IsAvailable { get; private set; }

        internal P2PNetworkRuntime(
            SteamNetworkLobby lobby,
            SteamNetworkTransport transport,
            INetworkSession session,
            INetworkObjectIdAllocator idAllocator,
            SteamNetworkPeerConnector peerConnector,
            INetworkMessageManager messageManager,
            INetworkObjectManager objectManager,
            INetworkObjectFactory objectFactory,
            INetworkVariableSyncScheduler variableScheduler,
            INetworkRpcSender rpcSender) {

            _transport = transport;
        }

        void IInitializable.Initialize() {
            try {
                if (!_transport.Start()) {
                    this.LogError("P2P runtime: Steam transport failed to start. P2P is disabled.");
                    return;
                }

                IsAvailable = true;
                this.Log("P2P runtime started.");
            } catch (Exception exception) {
                IsAvailable = false;
                _transport.Stop();
                this.LogError($"P2P runtime failed to start. P2P is disabled: {exception}");
            }
        }

        void IDisposable.Dispose() {
            IsAvailable = false;
            _transport.Stop();
        }
    }
}
