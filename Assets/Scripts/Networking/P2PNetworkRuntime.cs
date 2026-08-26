using System;
using UnityEngine.PlayerLoop;
using VContainer.Unity;
using Xoderony.Logging;
using Xoderony.Networking;
using Xoderony.Unity;

namespace Expriverse.Networking.P2P {
    /// <summary>根容器中启动并持有 P2P 技术验证栈。</summary>
    public sealed class P2PNetworkRuntime : IInitializable, IDisposable {
        private readonly SteamNetworkTransport _transport;
        private readonly NetworkVariableModule _variableModule;
        private bool _skipNextFixedTick;

        public bool IsAvailable { get; private set; }

        internal P2PNetworkRuntime(
            SteamNetworkLobby lobby,
            SteamNetworkTransport transport,
            INetworkSession session,
            INetworkObjectIdAllocator idAllocator,
            INetworkMessageManager messageManager,
            INetworkObjectManager objectManager,
            INetworkObjectFactory objectFactory,
            NetworkVariableModule variableModule,
            NetworkRpcModule rpcModule) {
            _transport = transport;
            _variableModule = variableModule;
        }

        void IInitializable.Initialize() {
            try {
                if (!_transport.Start()) {
                    this.LogError("P2P runtime: Steam transport failed to start. P2P is disabled.");
                    return;
                }

                PostUpdateLoop<FixedUpdate.ScriptRunDelayedFixedFrameRate>.Register(OnPostFixedUpdate);
                IsAvailable = true;
                this.Log("P2P runtime started.");
            } catch (Exception exception) {
                IsAvailable = false;
                PostUpdateLoop<FixedUpdate.ScriptRunDelayedFixedFrameRate>.Unregister(OnPostFixedUpdate);
                _transport.Stop();
                this.LogError($"P2P runtime failed to start. P2P is disabled: {exception}");
            }
        }

        void IDisposable.Dispose() {
            IsAvailable = false;
            PostUpdateLoop<FixedUpdate.ScriptRunDelayedFixedFrameRate>.Unregister(OnPostFixedUpdate);
            _transport.Stop();
        }

        private void OnPostFixedUpdate() {
            if (_skipNextFixedTick = !_skipNextFixedTick) {
                return;
            }

            _variableModule.Flush();
        }
    }
}
