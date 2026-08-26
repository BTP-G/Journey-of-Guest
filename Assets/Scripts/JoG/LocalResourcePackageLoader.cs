using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using YooAsset;

namespace JoG {

    internal sealed class LocalResourcePackageLoader : IResourcePackageLoader {
        public async UniTask<ResourcePackage> LoadPackageAsync(string packageName, string packageRoot = null, CancellationToken cancellationToken = default) {
            cancellationToken.ThrowIfCancellationRequested();
            EnsureYooAssetsInitialized();

            var package = GetOrCreatePackage(packageName);
            if (package.InitializeStatus != EOperationStatus.Succeeded) {
                var initializeOptions = CreateInitializeOptions(packageName, packageRoot);
                var initializeOperation = package.InitializePackageAsync(initializeOptions);
                await initializeOperation;
                if (initializeOperation.Status != EOperationStatus.Succeeded) {
                    throw new InvalidOperationException($"[ResourcePackage: {packageName}] Initialization failed: {initializeOperation.Error}");
                }
            }

            cancellationToken.ThrowIfCancellationRequested();
            var requestPackageVersionOperation = package.RequestPackageVersionAsync();
            await requestPackageVersionOperation;
            if (requestPackageVersionOperation.Status != EOperationStatus.Succeeded) {
                throw new InvalidOperationException($"[ResourcePackage: {packageName}] Request package version failed: {requestPackageVersionOperation.Error}");
            }

            cancellationToken.ThrowIfCancellationRequested();
            var loadPackageManifestOptions = new LoadPackageManifestOptions(requestPackageVersionOperation.PackageVersion, 60);
            var loadPackageManifestOperation = package.LoadPackageManifestAsync(loadPackageManifestOptions);
            await loadPackageManifestOperation;
            if (loadPackageManifestOperation.Status != EOperationStatus.Succeeded) {
                throw new InvalidOperationException($"[ResourcePackage: {packageName}] Load package manifest failed: {loadPackageManifestOperation.Error}");
            }

            cancellationToken.ThrowIfCancellationRequested();
            return package;
        }

        public async UniTask UnloadPackageAsync(ResourcePackage package, CancellationToken cancellationToken = default) {
            cancellationToken.ThrowIfCancellationRequested();
            var operation = package.DestroyPackageAsync();
            await operation;
            if (operation.Status != EOperationStatus.Succeeded) {
                throw new InvalidOperationException($"[ResourcePackage: {package.PackageName}] Destroy failed: {operation.Error}");
            }

            YooAssets.RemovePackage(package.PackageName);
            cancellationToken.ThrowIfCancellationRequested();
        }

        private static void EnsureYooAssetsInitialized() {
            if (!YooAssets.IsInitialized) {
                YooAssets.Initialize();
            }
        }

        private static ResourcePackage GetOrCreatePackage(string packageName) {
            return YooAssets.TryGetPackage(packageName, out var package) ? package : YooAssets.CreatePackage(packageName);
        }

        private static InitializePackageOptions CreateInitializeOptions(string packageName, string packageRoot) {
#if UNITY_EDITOR
            var simulateBuildResult = EditorSimulateBuildInvoker.Build(packageName, (int)EBundleType.VirtualAssetBundle);
            var editorFileSystem = FileSystemParameters.CreateDefaultEditorFileSystemParameters(simulateBuildResult.PackageRootDirectory);
            return new EditorSimulateModeOptions {
                EditorFileSystemParameters = editorFileSystem,
            };
#else
            var builtinFileSystem = FileSystemParameters.CreateDefaultBuiltinFileSystemParameters(packageRoot);
            return new OfflinePlayModeOptions {
                BuiltinFileSystemParameters = builtinFileSystem,
            };
#endif
        }
    }
}
