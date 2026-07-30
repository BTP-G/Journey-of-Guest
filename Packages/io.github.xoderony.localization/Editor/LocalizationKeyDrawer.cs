using Xoderony.Localization;
using Xoderony.UIElements;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(LocalizationKeyAttribute))]
public class LocalizationKeyDrawer : PropertyDrawer {

    private static readonly Regex keyValueRegex = new(
        @"^(?:""(?<quoted>[^""]+)""|'(?<single>[^']+)'|(?<plain>[A-Za-z0-9_\-\.]+))\s*[:=]\s*(?<value>.*)$",
        RegexOptions.Compiled
    );

    private static readonly string localizationFilePath = Path.Combine(Application.streamingAssetsPath, "Localization", "zh-CN.hjson");

    private static readonly List<string> keys = new();

    public override VisualElement CreatePropertyGUI(SerializedProperty property) {
        if (property.propertyType != SerializedPropertyType.String) {
            return new Label($"{nameof(LocalizationKeyAttribute)} 只能用于 string 字段");
        }

        var attr = attribute as LocalizationKeyAttribute;
        LoadKeys(attr.RegexPattern, property.stringValue, keys);

        var field = new SearchableDropdownField(property.displayName, keys) {
            Value = property.stringValue,
            tooltip = property.tooltip,
        };

        field.OnValueChanged += newValue => {
            property.stringValue = newValue;
            property.serializedObject.ApplyModifiedProperties();
        };

        return field;
    }

    private static void LoadKeys(string regexPattern, string currentValue, List<string> keys) {
        ReadKeysFromFile(keys);

        if (!string.IsNullOrWhiteSpace(regexPattern)) {
            try {
                var regex = new Regex(regexPattern);
                keys.RemoveAll(key => !regex.IsMatch(key));
            } catch (Exception e) {
                Debug.LogWarning($"LocalizableString 正则过滤失败：{regexPattern}\n{e.Message}");
            }
        }

        if (!string.IsNullOrWhiteSpace(currentValue) && !keys.Contains(currentValue)) {
            keys.Insert(0, currentValue);
        }

        keys.Sort(StringComparer.Ordinal);
    }

    private static void ReadKeysFromFile(List<string> keys) {
        var fullPath = localizationFilePath;

        if (!File.Exists(fullPath)) {
            Debug.LogWarning($"LocalizableString 找不到本地化文件：{fullPath}");
            return;
        }

        var scopes = new List<string>();
        string pendingScope = null;

        foreach (string rawLine in File.ReadLines(fullPath)) {
            string line = rawLine.Trim();

            if (string.IsNullOrWhiteSpace(line)) {
                continue;
            }

            if (line.StartsWith("//") || line.StartsWith("#")) {
                continue;
            }

            while (line.StartsWith("}")) {
                if (scopes.Count > 0) {
                    scopes.RemoveAt(scopes.Count - 1);
                }

                line = line[1..].TrimStart(',', ' ', '\t');

                if (string.IsNullOrWhiteSpace(line)) {
                    break;
                }
            }

            if (string.IsNullOrWhiteSpace(line)) {
                continue;
            }

            if (line.StartsWith("{")) {
                if (!string.IsNullOrWhiteSpace(pendingScope)) {
                    scopes.Add(pendingScope);
                    pendingScope = null;
                }

                continue;
            }

            Match match = keyValueRegex.Match(line);

            if (!match.Success) {
                continue;
            }

            var key = GetKey(match);
            var valuePart = match.Groups["value"].Value.Trim();

            if (string.IsNullOrWhiteSpace(key)) {
                continue;
            }

            if (string.IsNullOrWhiteSpace(valuePart)) {
                pendingScope = key;
                continue;
            }

            if (valuePart.StartsWith("{")) {
                scopes.Add(key);
                pendingScope = null;
                continue;
            }

            var fullKey = BuildFullKey(scopes, key);

            if (!string.IsNullOrWhiteSpace(fullKey) && !keys.Contains(fullKey)) {
                keys.Add(fullKey);
            }
        }

        static string GetKey(Match match) {
            if (match.Groups["quoted"].Success) {
                return match.Groups["quoted"].Value;
            }

            if (match.Groups["single"].Success) {
                return match.Groups["single"].Value;
            }

            return match.Groups["plain"].Value;
        }

        static string BuildFullKey(List<string> scopes, string key) {
            if (scopes.Count == 0) {
                return key;
            }

            return string.Join(".", scopes) + "." + key;
        }
    }
}