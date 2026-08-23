using UnityEngine;
using VContainer;
using VContainer.Unity;
using Xoderony.Networking;

namespace JoG.Networking.P2P {
    /// <summary>使用 RootScope 容器实例化并注入 P2P 网络对象。</summary>
    public sealed class VContainerNetworkObjectFactory : INetworkObjectFactory {
        private readonly IObjectResolver _container;

        [Inject]
        internal VContainerNetworkObjectFactory(IObjectResolver container) {
            _container = container;
        }

        public NetworkObject Instantiate(NetworkObject prefab) {
            return _container.Instantiate(prefab);
        }

        public void Release(NetworkObject instance) {
            Object.Destroy(instance.gameObject);
        }
    }
}
