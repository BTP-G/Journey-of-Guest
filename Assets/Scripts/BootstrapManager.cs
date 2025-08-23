using Cysharp.Threading.Tasks;
using JoG.BuffSystem;
using JoG.DebugExtensions;
using JoG.InventorySystem;
using JoG.Localization;
using JoG.ResourcePackageExtensions;
using JoG.UI;
using JoG.Utility;
using System;
using System.Reflection;
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
            // 1. 初始化 Unity Services
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

            // 2. 初始化 YooAssets
            var package = YooAssetHelper.GetOrCreatePackage("DefaultPackage");
            while (package.InitializeStatus is not EOperationStatus.Succeed) {
                try {
                    await LoadingManager.Loading(package.InitializeAsync(), Localizer.GetString("message.initialize",nameof(YooAsset)));
                } catch (Exception e) {
                    Debug.LogException(e);
                }
                if (package.InitializeStatus is not EOperationStatus.Succeed) {
                    this.LogError("[BootstrapManager] Failed to initialize YooAssets. Game startup aborted.");
                    if (!await PopupManager.PopupConfirmAsync(Localizer.GetString("message.initialize.failOrRetry", nameof(YooAsset)))) {
                        Application.Quit();
                    }
                }
            }
            YooAssets.SetDefaultPackage(package);
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                package.InjectAssetHandles(assembly);
            }

            ItemCatalog.RegisterFromPackage(package);

            NetworkPrefabs prefabs = NetworkManager.Singleton.NetworkConfig.Prefabs;
            foreach (var assetInfo in package.GetAssetInfos("network_prefab")) {
                var ah = package.LoadAssetSync(assetInfo);
                if (ah.Status == EOperationStatus.Succeed) {
                    if (ah.AssetObject is GameObject go) {
                        if (go.TryGetComponent<NetworkObject>(out _)) {
                            prefabs.Add(new NetworkPrefab() { Prefab = go });
                        } else {
                            Debug.LogWarning($"[Asset: {ah.AssetObject}] '{assetInfo.AssetPath}' does not have a NetworkObject component. Skipping registration.");
                        }
                    } else {
                        Debug.LogWarning($"[Asset: {ah.AssetObject}] '{assetInfo.AssetPath}' is not a GameObject. Skipping registration.");
                    }
                }
                ah.Release();
            }

            // 3. 初始化 DI 容器
            var root = VContainerSettings.Instance.GetOrCreateRootLifetimeScopeInstance();
            root.Build();

            // 4. 注册 Buff
            BuffRegistrar.Register(Assembly.GetExecutingAssembly());

            // 5. 异步加载主场景
            await SceneManager.LoadSceneAsync("MainScene");
        }
    }
}