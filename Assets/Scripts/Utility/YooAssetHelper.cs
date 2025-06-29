using JoG.Attributes;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using YooAsset;

namespace JoG.Utility {

    public static class YooAssetHelper {
        internal static Dictionary<string, List<AssetHandle>> packageToHandles = new();

        static YooAssetHelper() {
            if (!YooAssets.Initialized) {
                YooAssets.Initialize();
            }
        }

        public static ResourcePackage GetOrCreatePackage(string packageName) {
            return YooAssets.TryGetPackage(packageName) ?? YooAssets.CreatePackage(packageName);
        }

        /// <summary>
        /// Injects loaded assets as AssetHandle into static fields marked with <see
        /// cref="FromAssetAttribute"/> in the target assembly. Field type must be AssetHandle.
        /// </summary>
        /// <param name="target">The assembly to inject into.</param>
        public static async void InjectAssetHandles(Assembly target) {
            foreach (var type in target.GetTypes()) {
                foreach (var field in type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)) {
                    var assetAttribute = field.GetCustomAttribute<FromAssetAttribute>();
                    if (assetAttribute is null) continue;
                    if (field.FieldType != typeof(AssetHandle)) {
                        Debug.LogWarning($"{type.FullName}.{field.Name} is marked with {nameof(FromAssetAttribute)} but is not of type {nameof(AssetHandle)}. Skipped.");
                        continue;
                    }
                    try {
                        var resourcePackage = YooAssets.TryGetPackage(assetAttribute.PackageName);
                        if (resourcePackage is null) {
                            Debug.LogWarning($"[Type: {type.FullName}.{field.Name}] Required asset \"{assetAttribute.AssetLocation}\" in package \"{assetAttribute.PackageName}\" is not loaded!");
                            continue;
                        }
                        Debug.Log($"[ResourcePackage: {resourcePackage.PackageName}] Injecting AssetHandle for \"{assetAttribute.AssetLocation}\" to {type.FullName}.{field.Name}");
                        var ah = resourcePackage.LoadAssetAsync(assetAttribute.AssetLocation);
                        await ah.Task;
                        field.SetValue(null, ah);
                        Debug.Log($"[ResourcePackage: {resourcePackage.PackageName}] Injected AssetHandle to {type.FullName}.{field.Name}");
                        if (!packageToHandles.TryGetValue(resourcePackage.PackageName, out var assetHandles)) {
                            packageToHandles.Add(resourcePackage.PackageName, assetHandles = new List<AssetHandle>());
                        }
                        assetHandles.Add(ah);
                    } catch (Exception e) {
                        Debug.LogException(e);
                    }
                }
            }
        }

        public static async void UnloadResourcePackage(ResourcePackage resourcePackage) {
            if (resourcePackage == null) return;
            if (packageToHandles.Remove(resourcePackage.PackageName, out var assetHandles)) {
                foreach (var assetHandle in assetHandles) {
                    assetHandle.Release();
                }
            }
            var destroyOperation = resourcePackage.DestroyAsync();
            await destroyOperation.Task;
            if (destroyOperation.Status is not EOperationStatus.Succeed) {
                Debug.LogError($"Failed to destroy resource package: {resourcePackage.PackageName}");
            } else {
                Debug.Log($"Successfully destroyed resource package: {resourcePackage.PackageName}");
            }
        }
    }
}