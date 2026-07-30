using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using YooAsset;

namespace Xoderony.YooAsset {

    public static class YooAssetUtility {

        static YooAssetUtility() {
            if (!YooAssets.Initialized) {
                YooAssets.Initialize();
            }
        }

        public static ResourcePackage GetOrCreatePackage(string packageName) {
            return YooAssets.TryGetPackage(packageName) ?? YooAssets.CreatePackage(packageName);
        }

        public static async UniTask<ResourcePackage> CreatePackageAsync(string packageName, string packageRoot = null) {
            var package = YooAssets.CreatePackage(packageName);
            InitializeParameters initParameters;
#if UNITY_EDITOR
            var simulateBuildResult = EditorSimulateModeHelper.SimulateBuild(package.PackageName);
            var editorFileSystem = FileSystemParameters.CreateDefaultEditorFileSystemParameters(simulateBuildResult.PackageRootDirectory);
            initParameters = new EditorSimulateModeParameters {
                EditorFileSystemParameters = editorFileSystem,
            };
#else
            var buildinFileSystem = FileSystemParameters.CreateDefaultBuildinFileSystemParameters(packageRoot: packageRoot);
            initParameters = new OfflinePlayModeParameters {
                BuildinFileSystemParameters = buildinFileSystem,
            };
#endif
            var operation = package.InitializeAsync(initParameters);
            await operation.Task;
            if (operation.Status != EOperationStatus.Succeed) {
                throw new Exception($"[ResourcePackage: {packageName}] Initialization failed: {operation.Error}");
            }
            var requestPackageVersionOperation = package.RequestPackageVersionAsync();
            await requestPackageVersionOperation.Task;
            if (requestPackageVersionOperation.Status != EOperationStatus.Succeed) {
                throw new InvalidOperationException($"[ResourcePackage: {packageName}] Request package version failed.");
            }
            var updatePackageManifestOperation = package.UpdatePackageManifestAsync(requestPackageVersionOperation.PackageVersion);
            await updatePackageManifestOperation.Task;
            if (updatePackageManifestOperation.Status != EOperationStatus.Succeed) {
                throw new InvalidOperationException($"[ResourcePackage: {packageName}] Update package manifest failed: {updatePackageManifestOperation.Error}");
            }
            return package;
        }

        public static async UniTask DestroyPackageAsync(ResourcePackage package) {
            var operation = package.DestroyAsync();
            await operation.Task;
            if (operation.Status != EOperationStatus.Succeed) {
                throw new Exception($"[ResourcePackage: {package.PackageName}] Destroy failed.");
            }
            YooAssets.RemovePackage(package);
        }
    }
}
