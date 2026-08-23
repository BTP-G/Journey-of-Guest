using Cysharp.Threading.Tasks;
using JoG.Networking.P2P;
using JoG.Utilities;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using VContainer;
using Xoderony.Logging;
using Xoderony.Networking;
using Xoderony.YooAsset;
using YooAsset;

namespace JoG {

    internal class DefaultPackageManager : IAsyncBootstrapModule, IDisposable {
        private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(5);

        [Inject] internal INetworkObjectManager _networkObjectManager;

        private readonly List<JoGNetworkObject> _registeredP2PPrefabs = new();
        private readonly List<AssetHandle> _registeredP2PPrefabHandles = new();
        private ResourcePackage _package;

        async UniTask IAsyncBootstrapModule.InitializeAsync(CancellationToken cancellationToken) {
            while (true) {
                cancellationToken.ThrowIfCancellationRequested();
                try {
                    _package = await YooAssetUtility.CreatePackageAsync("DefaultPackage");
                    var handles = AssetsUtility.LoadDataFromPackage(_package);
                    foreach (var handle in handles) {
                        if (handle.AssetObject is not GameObject prefab || !prefab.TryGetComponent<JoGNetworkObject>(out var networkObject)) {
                            continue;
                        }

                        _networkObjectManager.RegisterPrefab(networkObject);
                        _registeredP2PPrefabs.Add(networkObject);
                        _registeredP2PPrefabHandles.Add(handle);
                    }
                    cancellationToken.ThrowIfCancellationRequested();
                    return;
                } catch (OperationCanceledException) {
                    UnloadPackage();
                    throw;
                } catch (Exception exception) {
                    UnloadPackage();
                    this.LogException(exception);
                    await UniTask.Delay(RetryDelay, cancellationToken: cancellationToken);
                }
            }
        }

        void IDisposable.Dispose() {
            UnloadPackage();
        }

        private void UnloadPackage() {
            UnregisterP2PPrefabs();
            if (_package == null) {
                return;
            }

            AssetsUtility.UnloadDataFromPackage(_package);
            _package = null;
        }

        private void UnregisterP2PPrefabs() {
            foreach (var prefab in _registeredP2PPrefabs) {
                _networkObjectManager.UnregisterPrefab(prefab);
            }

            _registeredP2PPrefabs.Clear();
            _registeredP2PPrefabHandles.Clear();
        }
    }
}
