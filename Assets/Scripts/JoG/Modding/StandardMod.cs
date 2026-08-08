using Cysharp.Threading.Tasks;
using JoG.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using Xoderony.Localization;
using Xoderony.Logging;
using Xoderony.YooAsset;
using YooAsset;

namespace JoG.Modding {

    public class StandardMod : Mod {
        public ResourcePackage ResourcePackage { get; private set; }

        protected override async UniTask OnEnableAsync() {
            await LoadResourcePackage();
            LoadAssets();
            Localizer.LanguageBuilders += BuildLanguage;
        }

        protected override async UniTask OnDisableAsync() {
            Localizer.LanguageBuilders -= BuildLanguage;
            UnloadAssets();
            await UnloadResourcePackage();
        }

        private void BuildLanguage(string languageCode, IDictionary<string, string> builder) {
            var path = Path.Combine(RootDirectory, "Localization", $"fallback.hjson");
            if (File.Exists(path)) {
                AssetsUtility.LoadLanguageFromHjson(path, builder);
            }
            path = Path.Combine(RootDirectory, "Localization", $"{languageCode}.hjson");
            if (File.Exists(path)) {
                AssetsUtility.LoadLanguageFromHjson(path, builder);
            }
        }

        private async UniTask LoadResourcePackage() {
            var packageDirectory = Path.Combine(RootDirectory, "YooAssetPackage");
            if (!Directory.Exists(packageDirectory)) {
                return;
            }
            try {
                var packageName = Id + "Package";
                var package = await YooAssetUtility.CreatePackageAsync(packageName, packageDirectory);
                ResourcePackage = package;
            } catch (Exception ex) {
                this.LogError($"[Id: {Id}] Failed to load resource package from '{packageDirectory}': {ex}");
            }
        }

        private async UniTask UnloadResourcePackage() {
            if (ResourcePackage == null) {
                return;
            }

            try {
                await YooAssetUtility.DestroyPackageAsync(ResourcePackage);
            } catch (Exception ex) {
                this.LogError($"[Id: {Id}] Failed to unload resource package '{ResourcePackage.PackageName}': {ex}");
            } finally {
                ResourcePackage = null;
            }
        }

        private void LoadAssets() {
            var package = ResourcePackage;
            if (package == null) {
                return;
            }

            AssetsUtility.LoadDataFromPackage(package);
        }

        private void UnloadAssets() {
            var package = ResourcePackage;
            if (package == null) {
                return;
            }

            AssetsUtility.UnloadDataFromPackage(package);
        }
    }
}
