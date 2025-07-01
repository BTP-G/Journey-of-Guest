using Hjson;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace JoG.Localization {

    public static class LocalizationManager {
        private static Dictionary<string, string> _table = new();
        private static string _currentLanguage = "zh-CN";

        public static string CurrentLanguage {
            get => _currentLanguage;
            set {
                if (_currentLanguage == value) return;
                _currentLanguage = value;
                Load();
            }
        }

        public static string GetString(string key) {
            return _table.TryGetValue(key, out var value) ? value : string.Empty;
        }

#if UNITY_EDITOR

        [MenuItem("Tools/Localization/Init")]
#endif
        [RuntimeInitializeOnLoadMethod]
        private static void Load() {
            var table = HjsonLoader.LoadLocalization(_currentLanguage);
            if (table is null) {
                Debug.LogError($"Localization file for language '{_currentLanguage}' not found or invalid.");
                return;
            }
            var config = HjsonLoader.LoadConfig();
            if (config is null) {
                Debug.LogError("Config file not found or invalid.");
                return;
            }
            _table.Clear();
            // 将table中的键值对转换为字符串并存储到_table中，同时将字符串中的占位符替换为config中的对应值
            foreach (var kv in table) {
                if (kv.Value.JsonType is not JsonType.String) {
                    continue;
                }
                var value = kv.Value.ToString();
                // 获取当前key的路径（如 buff.bleeding.desc -> buff.bleeding）
                var keyPath = kv.Key;
                var lastDotIndex = keyPath.LastIndexOf('.');
                var parentPath = lastDotIndex > 0 ? keyPath[..lastDotIndex] : keyPath;

                // 查找该路径下所有config占位符 例如 {tickInterval}
                value = System.Text.RegularExpressions.Regex.Replace(
                    value,
                    @"\{([^\{\}]+)\}",
                    match => {
                        var placeholder = match.Groups[1].Value;
                        // 先尝试在同路径下查找
                        var configKey = $"{parentPath}.{placeholder}";
                        if (config.TryGetValue(configKey, out var configValue)) {
                            return configValue.ToString();
                        }
                        // 再尝试全局查找
                        if (config.TryGetValue(placeholder, out configValue)) {
                            return configValue.ToString();
                        }
                        // 找不到就原样返回
                        return match.Value;
                    }
                );
                _table[kv.Key] = value;
                Debug.Log($"Localization entry '{kv.Key}' loaded with value: {value}");
            }
            Debug.Log($"Localization loaded for language '{_currentLanguage}' with {_table.Count} entries.");
        }
    }
}