using Cysharp.Threading.Tasks;
using JoG.Utilities;
using System;
using Unity.Netcode;
using VContainer;
using Xoderony.YooAsset;
using YooAsset;

namespace JoG {

    internal class DefaultPackageManager : IAsyncBootstrapModule, IDisposable {
        [Inject] internal NetworkManager _networkManager;
        private ResourcePackage package;

        async UniTask IAsyncBootstrapModule.InitializeAsync() {
            package = await YooAssetUtility.CreatePackageAsync("DefaultPackage");
            AssetsUtility.LoadDataFromPackage(package);
        }

        void IDisposable.Dispose() {
            AssetsUtility.UnloadDataFromPackage(package);
        }
    }
}
