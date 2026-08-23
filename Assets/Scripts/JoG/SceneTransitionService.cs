using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine.SceneManagement;
using YooAsset;

namespace JoG {
    /// <summary>由 RootScope 持有的 YooAsset 场景加载、激活和卸载编排。</summary>
    internal sealed class SceneTransitionService : IDisposable {
        private const string DefaultPackageName = "DefaultPackage";
        private const string MainSceneLocation = "MainScene";

        private SceneHandle _currentSceneHandle;

        public UniTask LoadMainSceneAsync(CancellationToken cancellationToken = default) {
            var package = YooAssets.GetPackage(DefaultPackageName);
            if (package == null) {
                throw new InvalidOperationException($"YooAsset package '{DefaultPackageName}' is not available.");
            }

            return LoadSceneAsync(package, MainSceneLocation, cancellationToken);
        }

        public async UniTask LoadSceneAsync(ResourcePackage package, string location, CancellationToken cancellationToken = default) {
            cancellationToken.ThrowIfCancellationRequested();
            var previousScene = SceneManager.GetActiveScene();
            var previousHandle = GetCurrentSceneHandle(previousScene);
            var handle = package.LoadSceneAsync(
                location,
                LoadSceneMode.Additive,
                LocalPhysicsMode.None,
                allowSceneActivation: false);
            var sceneActivated = false;

            try {
                await UniTask.WaitUntil(
                    () => handle.Progress >= 0.9f || handle.Status == EOperationStatus.Failed);
                if (handle.Status != EOperationStatus.Failed) {
                    handle.AllowSceneActivation();
                    await handle;
                }

                if (handle.Status != EOperationStatus.Succeeded) {
                    throw new InvalidOperationException($"Failed to load scene '{location}': {handle.Error}");
                }

                if (!handle.ActivateScene()) {
                    throw new InvalidOperationException($"Failed to activate scene '{location}'.");
                }

                sceneActivated = true;
                _currentSceneHandle = handle;
                await UnloadPreviousSceneAsync(previousScene, previousHandle);
            } catch {
                if (!sceneActivated && handle.IsValid) {
                    handle.Release();
                }
                throw;
            }
        }

        public void Dispose() {
            var handle = _currentSceneHandle;
            _currentSceneHandle = null;
            if (handle == null || !handle.IsValid) {
                return;
            }

            if (handle.SceneObject.IsValid() && handle.SceneObject.isLoaded) {
                handle.UnloadSceneAsync();
            } else {
                handle.Release();
            }
        }

        private SceneHandle GetCurrentSceneHandle(Scene scene) {
            var handle = _currentSceneHandle;
            if (handle == null) {
                return null;
            }

            if (!handle.IsValid) {
                _currentSceneHandle = null;
                return null;
            }

            if (!handle.SceneObject.IsValid() || !handle.SceneObject.isLoaded) {
                handle.Release();
                _currentSceneHandle = null;
                return null;
            }

            return handle.SceneObject == scene ? handle : null;
        }

        private async UniTask UnloadPreviousSceneAsync(Scene previousScene, SceneHandle previousHandle) {
            if (!previousScene.IsValid() || !previousScene.isLoaded || previousScene == SceneManager.GetActiveScene()) {
                return;
            }

            if (previousHandle != null && previousHandle.IsValid && previousHandle.SceneObject == previousScene) {
                var unloadOperation = previousHandle.UnloadSceneAsync();
                await unloadOperation;
                if (_currentSceneHandle == previousHandle) {
                    _currentSceneHandle = null;
                }
                return;
            }

            var unloadSceneOperation = SceneManager.UnloadSceneAsync(previousScene);
            if (unloadSceneOperation != null) {
                await unloadSceneOperation.ToUniTask();
            }
        }
    }
}
