using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using JoG.Attributes;

namespace JoG {

    public static class ConfigManager {
#if UNITY_EDITOR

        [MenuItem("Tools/Config/Inject")]
#endif
        [RuntimeInitializeOnLoadMethod]
        public static void Inject() {
            var config = HjsonLoader.LoadConfig();
            if (config is null) {
                Debug.LogError("Config not loaded, injection aborted.");
                return;
            }
            var injectedCount = 0;
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().AsSpan()) {
                foreach (var type in assembly.GetTypes()) {
                    foreach (var field in type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).AsSpan()) {
                        var attr = field.GetCustomAttribute<FromConfigAttribute>();
                        if (attr is null) continue;
                        if (!config.TryGetValue(attr.Key, out var value)) {
                            Debug.LogWarning($"Config key '{attr.Key}' not found for {type.FullName}.{field.Name}");
                            continue;
                        }
                        try {
                            var converted = Convert.ChangeType(value.ToString(), field.FieldType);
                            field.SetValue(null, converted);
                            injectedCount++;
                            Debug.Log($"[Config Inject] {type.FullName}.{field.Name} <= {converted}");
                        } catch (Exception ex) {
                            Debug.LogError($"Failed to inject config for {type.FullName}.{field.Name}: {ex.Message}");
                        }
                    }
                }
            }
            Debug.Log($"Config injection complete. Injected {injectedCount} field(s).");
        }
    }
}