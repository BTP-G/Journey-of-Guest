using Cysharp.Threading.Tasks;
using System;
using YooAsset;

namespace Xoderony.YooAsset {

    public static class YooAssetUtility {

        static YooAssetUtility() {
            if (!YooAssets.IsInitialized) {
                YooAssets.Initialize();
            }
        }

        public static ResourcePackage GetOrCreatePackage(string packageName) {
            return YooAssets.TryGetPackage(packageName, out var package) ? package : YooAssets.CreatePackage(packageName);
        }

        public static async UniTask<ResourcePackage> CreatePackageAsync(string packageName, string packageRoot = null) {
            var package = GetOrCreatePackage(packageName);
            if (package.InitializeStatus != EOperationStatus.Succeeded) {
                InitializePackageOptions initParameters;
#if UNITY_EDITOR
                var simulateBuildResult = EditorSimulateBuildInvoker.Build(packageName, (int)EBundleType.VirtualAssetBundle);
                var editorFileSystem = FileSystemParameters.CreateDefaultEditorFileSystemParameters(simulateBuildResult.PackageRootDirectory);
                initParameters = new EditorSimulateModeOptions {
                    EditorFileSystemParameters = editorFileSystem,
                };
#else
                var buildinFileSystem = FileSystemParameters.CreateDefaultBuiltinFileSystemParameters(packageRoot);
                initParameters = new OfflinePlayModeOptions {
                    BuiltinFileSystemParameters = buildinFileSystem,
                };
#endif
                var initializeOperation = package.InitializePackageAsync(initParameters);
                await initializeOperation;
                if (initializeOperation.Status != EOperationStatus.Succeeded) {
                    throw new Exception($"[ResourcePackage: {packageName}] Initialization failed: {initializeOperation.Error}");
                }
            }
            var requestPackageVersionOperation = package.RequestPackageVersionAsync();
            await requestPackageVersionOperation;
            if (requestPackageVersionOperation.Status != EOperationStatus.Succeeded) {
                throw new InvalidOperationException($"[ResourcePackage: {packageName}] Request package version failed.");
            }
            var loadPackageManifestOperation = package.LoadPackageManifestAsync(new LoadPackageManifestOptions(requestPackageVersionOperation.PackageVersion, 60));
            await loadPackageManifestOperation;
            if (loadPackageManifestOperation.Status != EOperationStatus.Succeeded) {
                throw new InvalidOperationException($"[ResourcePackage: {packageName}] Load package manifest failed: {loadPackageManifestOperation.Error}");
            }
            return package;
        }

        public static async UniTask DestroyPackageAsync(ResourcePackage package) {
            var operation = package.DestroyPackageAsync();
            await operation;
            if (operation.Status != EOperationStatus.Succeeded) {
                throw new Exception($"[ResourcePackage: {package.PackageName}] Destroy failed.");
            }
            YooAssets.RemovePackage(package.PackageName);
        }
    }
}
