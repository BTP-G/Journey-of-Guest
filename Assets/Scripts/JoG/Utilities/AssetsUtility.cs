using Hjson;
using JoG.Character;
using JoG.GameplayEffects;
using JoG.Item;
using System;
using System.Collections.Generic;
using UnityEngine;
using Xoderony.Extensions;
using Xoderony.GameplayEffects;
using YooAsset;

namespace JoG.Utilities {

    public static class AssetsUtility {
        private static Dictionary<ResourcePackage, List<AssetHandle>> _packageToHandles = new();

        public static void LoadDataFromPackage(ResourcePackage package) {
            if (_packageToHandles.TryGetValue(package, out var handles)) {
                Debug.LogError($"Has load this package: {package}");
                return;
            }
            _packageToHandles[package] = handles = new List<AssetHandle>();
            var tags = new[] { "item_data", "character_data", "gameplay_effect_def", "periodic_health_change_def" };
            foreach (var assetInfo in package.GetAssetInfos(tags)) {
                AssetHandle ah = null;
                try {
                    ah = package.LoadAssetSync(assetInfo);
                    if (ah.Status != EOperationStatus.Succeeded) {
                        throw new Exception($"[{nameof(AssetsUtility)}: Loaded asset '{assetInfo.AssetPath}' from package '{package.PackageName}' failed: {ah.Error}");
                    }
                    if (ah.AssetObject is ItemData itemData) {
                        GameplayEffectDefinitionRegistry.Shared.Add(itemData);
                        ItemDataDictionary.Shared.Add(itemData);
                    } else if (ah.AssetObject is CharacterData characterData) {
                        CharacterDataDictionary.Shared.Add(characterData);
                    } else if (ah.AssetObject is GameplayEffectDefinition effectDefinition) {
                        GameplayEffectDefinitionRegistry.Shared.Add(effectDefinition);
                    } else if (ah.AssetObject is PeriodicHealthChangeDefinition periodicHealthChangeDefinition) {
                        PeriodicHealthChangeDefinitionDictionary.Shared.Add(periodicHealthChangeDefinition);
                    } else {
                        throw new Exception($"[{nameof(AssetsUtility)}: Loaded asset '{assetInfo.AssetPath}' from package '{package.PackageName}' is of unsupported type '{ah.AssetObject.GetType().FullName}'.");
                    }
                    handles.Add(ah);
                } catch (Exception ex) {
                    Debug.LogException(ex);
                    ah?.Release();
                }
            }
        }

        public static void UnloadDataFromPackage(ResourcePackage package) {
            if (!_packageToHandles.Remove(package, out var handles)) {
                Debug.LogError($"Has't load this package: {package}");
                return;
            }
            foreach (var ah in handles) {
                try {
                    if (ah.AssetObject is ItemData itemData) {
                        ItemDataDictionary.Shared.Remove(itemData);
                        GameplayEffectDefinitionRegistry.Shared.Remove(itemData);
                    } else if (ah.AssetObject is CharacterData characterData) {
                        CharacterDataDictionary.Shared.Remove(characterData);
                    } else if (ah.AssetObject is GameplayEffectDefinition effectDefinition) {
                        GameplayEffectDefinitionRegistry.Shared.Remove(effectDefinition);
                    } else if (ah.AssetObject is PeriodicHealthChangeDefinition periodicHealthChangeDefinition) {
                        PeriodicHealthChangeDefinitionDictionary.Shared.Remove(periodicHealthChangeDefinition);
                    }
                } catch (Exception ex) {
                    Debug.LogException(ex);
                } finally {
                    ah.Release();
                }
            }
            handles.Clear();
        }

        public static void LoadLanguageFromHjson(string languageFilePath, IDictionary<string, string> builder) {
            try {
                var jv = HjsonValue.Load(languageFilePath);
                FlattenAndAdd(jv, string.Empty, builder);
            } catch (Exception ex) {
                Debug.LogWarningFormat("Failed to load language file at {0}: {1}", languageFilePath, ex);
            }
        }

        private static void FlattenAndAdd(JsonValue value, string prefix, IDictionary<string, string> target) {
            if (value is JsonObject jo) {
                foreach (var kvp in jo) {
                    var newPrefix = prefix.IsNullOrEmpty() ? kvp.Key : $"{prefix}.{kvp.Key}";
                    FlattenAndAdd(kvp.Value, newPrefix, target);
                }
            } else if (value.JsonType == JsonType.String) {
                target[prefix] = value.Qs();
            } else {
                Debug.LogWarning($"Skipped non-string entry in Hjson: {prefix} = {value}");
            }
        }
    }
}
