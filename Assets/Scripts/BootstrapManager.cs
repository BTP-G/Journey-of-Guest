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
            // 1. ��ʼ�� Unity Services
            while (UnityServices.State is not ServicesInitializationState.Initialized) {
                try {
                    await LoadingPanelManager.Loading(UnityServices.InitializeAsync(), "Initializing Unity Services...");
                } catch (System.Exception e) {
                    Debug.LogException(e);
                }
                if (UnityServices.State is not ServicesInitializationState.Initialized) {
                    Debug.LogError("[BootstrapManager] Failed to initialize Unity Services. Game startup aborted.");
                    ConfirmPopupManager.Popup("��ʼ��Unity����ʧ�ܣ��Ƿ����ԣ�ȡ�����˳���Ϸ��", cancelAction: () => {
                        Application.Quit();
                    });
                }
            }

            // 2. ��ʼ�� YooAssets
            var package = YooAssetHelper.GetOrCreatePackage("DefaultPackage");
            while (package.InitializeStatus is not EOperationStatus.Succeed) {
                try {
                    package.Initialize();
                } catch (System.Exception e) {
                    Debug.LogException(e);
                }
                if (package.InitializeStatus is not EOperationStatus.Succeed) {
                    Debug.LogError("[BootstrapManager] Failed to initialize YooAssets. Game startup aborted.");
                    ConfirmPopupManager.Popup("����Ĭ����Դ��ʧ�ܣ��Ƿ����ԣ�ȡ�����˳���Ϸ��", cancelAction: () => {
                        Application.Quit();
                    });
                }
            }
            YooAssets.SetDefaultPackage(package);

            // 3. ��ʼ�� DI ����
            var root = VContainerSettings.Instance.GetOrCreateRootLifetimeScopeInstance();
            root.Build();

            // 4. ע�� Buff
            BuffRegistrar.Register(Assembly.GetExecutingAssembly());

            // 5. �첽����������
            await SceneManager.LoadSceneAsync(1);
        }
    }
}