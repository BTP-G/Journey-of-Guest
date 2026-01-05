using Cysharp.Threading.Tasks;
using GuestUnion.UI;
using System;
using System.Collections.Generic;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;

namespace JoG {

    public class BootstrapManager : MonoBehaviour {
        public PopupManager PopupManager;

        private async void Awake() {
            using var scope = PopupManager.PopupLoader();
            while (UnityServices.State != ServicesInitializationState.Initialized) {
                try {
                    await UnityServices.InitializeAsync();
                } catch (Exception e) {
                    Debug.LogException(e);
                }
            }
            var root = VContainerSettings.Instance.GetOrCreateRootLifetimeScopeInstance();
            root.Build();
            foreach (var startable in root.Container.Resolve<IEnumerable<IAsyncBootstrapModule>>()) {
                await startable.InitializeAsync();
            }
            await SceneManager.LoadSceneAsync(1);
        }
    }
}