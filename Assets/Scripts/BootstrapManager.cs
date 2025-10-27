using Cysharp.Threading.Tasks;
using GuestUnion.Extensions.Unity;
using GuestUnion.Utilities.YooAsset;
using JoG.BuffSystem;
using JoG.InventorySystem;
using JoG.Localization;
using JoG.UI;
using System;
using Unity.Netcode;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer.Unity;
using YooAsset;

namespace JoG {

    public class BootstrapManager : MonoBehaviour {

        private async void Awake() {
            Configer.Initialize();
            Localizer.Initialize();
            while (UnityServices.State is not ServicesInitializationState.Initialized) {
                try {
                    await LoadingManager.Loading(
                        UnityServices.InitializeAsync(),
                        Localizer.GetString("message.initialize", nameof(UnityServices)));
                } catch (Exception e) {
                    Debug.LogException(e);
                }
                if (UnityServices.State is not ServicesInitializationState.Initialized) {
                    this.LogError("[BootstrapManager] Failed to initialize Unity Services. Game startup aborted.");
                    if (!await PopupManager.PopupConfirmAsync(Localizer.GetString("message.initialize.failOrRetry", nameof(UnityServices)))) {
                        Application.Quit();
                    }
                }
            }

            var package = YooAssetUtility.GetOrCreatePackage("DefaultPackage");
            while (package.InitializeStatus is not EOperationStatus.Succeed) {
                try {
                    await LoadingManager.Loading(package.InitializeAsync(), Localizer.GetString("message.initialize", nameof(YooAsset)));
                } catch (Exception e) {
                    Debug.LogException(e);
                }
                if (package.InitializeStatus is not EOperationStatus.Succeed) {
                    this.LogError("[BootstrapManager] Failed to initialize YooAssets. Game startup aborted.");
                    if (!await PopupManager.PopupConfirmAsync(Localizer.GetString("message.initialize.retry", nameof(YooAsset)))) {
                        Application.Quit();
                    }
                }
            }
            YooAssets.SetDefaultPackage(package);
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                await YooAssetUtility.InjectAssetAsync(assembly);
            }

            await ItemCatalog.RegisterFromPackageAsync(package);

            var prefabs = NetworkManager.Singleton.NetworkConfig.Prefabs;
            foreach (var assetInfo in package.GetAssetInfos("network_prefab")) {
                if (assetInfo.AssetType != typeof(GameObject)) {
                    this.LogWarning($"[AssetAddress: {assetInfo.Address}] '{assetInfo.AssetType}' is not GameObject. Skipping registration.");
                    continue;
                }
                var ah = package.LoadAssetAsync(assetInfo);
                await ah.Task;
                if (ah.Status == EOperationStatus.Succeed) {
                    var go = ah.AssetObject as GameObject;
                    if (go.TryGetComponent<NetworkObject>(out _)) {
                        prefabs.Add(new NetworkPrefab() { Prefab = go });
                    } else {
                        this.LogWarning($"[Asset: {ah.AssetObject}] '{assetInfo.AssetPath}' does not have a NetworkObject component. Skipping registration.");
                    }

                }
                ah.Release();
            }

            var root = VContainerSettings.Instance.GetOrCreateRootLifetimeScopeInstance();
            root.Build();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                BuffRegistrar.Register(assembly);
            }

            await SceneManager.LoadSceneAsync("MainScene");
        }
    }
}