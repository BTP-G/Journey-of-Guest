using JoG.Attributes;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using YooAsset;
using static JoG.Utility.YooAssetHelper;

namespace JoG.ResourcePackageExtensions {

    public static class ResourcePackageExtensions {

        /// <summary>
        /// Injects assets from this resource package into static fields marked with <see
        /// cref="FromAssetAttribute"/> in the target assembly. Field type must be AssetHandle.
        /// </summary>
        /// <param name="resourcePackage">The resource package to inject from.</param>
        /// <param name="target">The assembly to inject into.</param>
        public static async void InjectAssetHandles(this ResourcePackage resourcePackage, Assembly target) {
            foreach (var type in target.GetTypes()) {
                foreach (var field in type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)) {
                    var assetAttribute = field.GetCustomAttribute<FromAssetAttribute>();
                    if (assetAttribute is null || assetAttribute.PackageName != resourcePackage.PackageName)
                        continue;
                    if (field.FieldType != typeof(AssetHandle)) {
                        Debug.LogWarning($"[ResourcePackage: {resourcePackage.PackageName}] {type.FullName}.{field.Name} is marked with {nameof(FromAssetAttribute)} but is not of type {nameof(AssetHandle)}. Skipped.");
                        continue;
                    }
                    try {
                        Debug.Log($"[ResourcePackage: {resourcePackage.PackageName}] Injecting AssetHandle for \"{assetAttribute.AssetLocation}\" to {type.FullName}.{field.Name}");
                        var ah = resourcePackage.LoadAssetAsync(assetAttribute.AssetLocation);
                        await ah.Task;
                        field.SetValue(null, ah);
                        Debug.Log($"[ResourcePackage: {resourcePackage.PackageName}] Injected AssetHandle to {type.FullName}.{field.Name}");
                        if (!packageToHandles.TryGetValue(resourcePackage.PackageName, out var assetHandles)) {
                            packageToHandles.Add(resourcePackage.PackageName, assetHandles = new List<AssetHandle>());
                        }
                        assetHandles.Add(ah);
                    } catch (System.Exception e) {
                        Debug.LogError(e);
                    }
                }
            }
        }

        /// <summary>在单机模式下初始化资源包。</summary>
        /// <param name="package">要初始化的资源包</param>
        /// <param name="packageRoot">资源包根目录</param>
        public static async void Initialize(this ResourcePackage package, string packageRoot = null) {
            InitializeParameters initParameters;
#if UNITY_EDITOR
            var simulateBuildResult = EditorSimulateModeHelper.SimulateBuild(package.PackageName);
            var editorFileSystem = FileSystemParameters.CreateDefaultEditorFileSystemParameters(simulateBuildResult.PackageRootDirectory);
            initParameters = new EditorSimulateModeParameters {
                EditorFileSystemParameters = editorFileSystem
            };
#else
            var buildinFileSystem = FileSystemParameters.CreateDefaultBuildinFileSystemParameters(packageRoot: packageRoot);
            initParameters = new OfflinePlayModeParameters {
                BuildinFileSystemParameters = buildinFileSystem,
            };
#endif
            var operation = package.InitializeAsync(initParameters);
            await operation.Task;
            if (operation.Status is not EOperationStatus.Succeed) {
                Debug.LogError($"[ResourcePackage: {package.PackageName}] Initialization failed.");
            }
            var requestPackageVersionOperation = package.RequestPackageVersionAsync();
            await requestPackageVersionOperation.Task;
            if (requestPackageVersionOperation.Status is not EOperationStatus.Succeed) {
                Debug.LogError($"[ResourcePackage: {package.PackageName}] Request package version failed.");
            }
            var updatePackageManifestOperation = package.UpdatePackageManifestAsync(requestPackageVersionOperation.PackageVersion);
            await updatePackageManifestOperation.Task;
            if (updatePackageManifestOperation.Status is not EOperationStatus.Succeed) {
                Debug.LogError($"[ResourcePackage: {package.PackageName}] Update package manifest failed.");
            }
        }
    }
}