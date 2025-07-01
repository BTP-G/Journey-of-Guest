using Cysharp.Threading.Tasks;
using JoG.BuffSystem;
using JoG.DebugExtensions;
using JoG.InventorySystem;
using JoG.ResourcePackageExtensions;
using JoG.UI;
using JoG.Utility;
using System;
using System.Reflection;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer.Unity;
using YooAsset;

namespace JoG {

    public class BootstrapManager : MonoBehaviour {

        private async void Awake() {
            // 1. ��ʼ�� Unity Services
            while (UnityServices.State is not ServicesInitializationState.Initialized) {
                try {
                    await LoadingPanelManager.Loading(UnityServices.InitializeAsync(), "Initializing Unity Services...");
                } catch (System.Exception e) {
                    Debug.LogException(e);
                }
                if (UnityServices.State is not ServicesInitializationState.Initialized) {
                    this.LogError("[BootstrapManager] Failed to initialize Unity Services. Game startup aborted.");
                    if (!await PopupManager.PopupConfirmAsync("��ʼ��Unity����ʧ�ܣ��Ƿ����ԣ�ȡ�����˳���Ϸ��")) {
                        Application.Quit();
                    }
                }
            }

            // 2. ��ʼ�� YooAssets
            var package = YooAssetHelper.GetOrCreatePackage("DefaultPackage");
            while (package.InitializeStatus is not EOperationStatus.Succeed) {
                try {
                    await LoadingPanelManager.Loading(package.InitializeAsync(), $"Initializing ResourcePackage {package.PackageName}");
                } catch (System.Exception e) {
                    Debug.LogException(e);
                }
                if (package.InitializeStatus is not EOperationStatus.Succeed) {
                    this.LogError("[BootstrapManager] Failed to initialize YooAssets. Game startup aborted.");
                    if (!await PopupManager.PopupConfirmAsync("����Ĭ����Դ��ʧ�ܣ��Ƿ����ԣ�ȡ�����˳���Ϸ��")) {
                        Application.Quit();
                    }
                }
            }
            YooAssets.SetDefaultPackage(package);
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                package.InjectAssetHandles(assembly);
            }

            ItemCollector.RegisterFromPackage(package);

            // 3. ��ʼ�� DI ����
            var root = VContainerSettings.Instance.GetOrCreateRootLifetimeScopeInstance();
            root.Build();

            // 4. ע�� Buff
            BuffRegistrar.Register(Assembly.GetExecutingAssembly());

            // 5. �첽����������
            await SceneManager.LoadSceneAsync("MainScene");
        }
    }
}