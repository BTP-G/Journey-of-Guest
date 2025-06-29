using Cysharp.Threading.Tasks;
using JoG.BuffSystem;
using JoG.DebugExtensions;
using JoG.ResourcePackageExtensions;
using JoG.UI;
using JoG.Utility;
using System.Reflection;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer.Unity;
using YooAsset;

namespace JoG {

    public class BootstrapManager : MonoBehaviour {

        private async void Awake() {
            // 1. 初始化 Unity Services
            while (UnityServices.State is not ServicesInitializationState.Initialized) {
                try {
                    await LoadingPanelManager.Loading(UnityServices.InitializeAsync(), "Initializing Unity Services...");
                } catch (System.Exception e) {
                    Debug.LogException(e);
                }
                if (UnityServices.State is not ServicesInitializationState.Initialized) {
                    Debug.LogError("[BootstrapManager] Failed to initialize Unity Services. Game startup aborted.");
                    ConfirmPopupManager.Popup("初始化Unity服务失败，是否重试？取消将退出游戏。", cancelAction: () => {
                        Application.Quit();
                    });
                }
            }

            // 2. 初始化 YooAssets
            var package = YooAssetHelper.GetOrCreatePackage("DefaultPackage");
            while (package.InitializeStatus is not EOperationStatus.Succeed) {
                try {
                    package.Initialize();
                } catch (System.Exception e) {
                    Debug.LogException(e);
                }
                if (package.InitializeStatus is not EOperationStatus.Succeed) {
                    Debug.LogError("[BootstrapManager] Failed to initialize YooAssets. Game startup aborted.");
                    ConfirmPopupManager.Popup("加载默认资源包失败，是否重试？取消将退出游戏。", cancelAction: () => {
                        Application.Quit();
                    });
                }
            }
            YooAssets.SetDefaultPackage(package);

            // 3. 初始化 DI 容器
            var root = VContainerSettings.Instance.GetOrCreateRootLifetimeScopeInstance();
            root.Build();

            // 4. 注册 Buff
            BuffRegistrar.Register(Assembly.GetExecutingAssembly());

            // 5. 异步加载主场景
            await SceneManager.LoadSceneAsync(1);
        }
    }
}