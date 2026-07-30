#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using YooAsset.Editor;
using AssetInfo = YooAsset.Editor.AssetInfo;
using UIObjectField = UnityEditor.UIElements.ObjectField;

namespace Xoderony.YooAsset.Editor {

    [CustomPropertyDrawer(typeof(YooAssetReference<>), true)]
    public class YooAssetReferenceDrawer : PropertyDrawer {

        private static readonly Dictionary<string, HashSet<string>> _pkgToAddress = new();

        private static readonly Dictionary<string, List<AssetInfo>> _pkgToAssetInfo = new();

        private static readonly List<string> _packages = new();

        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            BuildCache();
            var root = new VisualElement();
            var cacheProp = property.FindPropertyRelative("_assetCache");
            var locationProp = property.FindPropertyRelative("_location");
            var packageProp = property.FindPropertyRelative("_packageName");
            if (string.IsNullOrWhiteSpace(packageProp.stringValue)) {
                packageProp.stringValue = _packages.FirstOrDefault();
                packageProp.serializedObject
                           .ApplyModifiedProperties();
            }
            var row = new VisualElement {
                style = {
                    flexDirection = FlexDirection.Row
                }
            };
            var content = new VisualElement {
                style = {
                    display = DisplayStyle.None
                }
            };
            var arrow = new Label("▶") {
                style = {
                    width = 12,
                    unityTextAlign = TextAnchor.MiddleCenter,
                    marginRight = 2,
                    fontSize = 9
                },
            };
            Type elementType = null;
            if (fieldInfo.FieldType.HasElementType) {
                elementType = fieldInfo.FieldType
                                       .GetElementType();
            } else {
                elementType = fieldInfo.FieldType;
            }
            while (elementType.IsGenericType) {
                elementType = elementType.GetGenericArguments()[0];
            }
            var cacheField = new UIObjectField(preferredLabel) {
                objectType = elementType,
                allowSceneObjects = false,
                style = {
                    flexGrow = 1
                },
                tooltip = property.tooltip,
            };
            cacheField.BindProperty(cacheProp);
            var selector = cacheField.Q(className: UIObjectField.selectorUssClassName);
            var selectorParent = selector.parent;
            var newSelectorButton = new Button(() => OpenResourcePicker(cacheField, packageProp.stringValue));
            newSelectorButton.AddToClassList(UIObjectField.selectorUssClassName);
            selector.RemoveFromHierarchy();
            selectorParent.Add(newSelectorButton);
            var packageField = new DropdownField("Package", _packages, 0);
            packageField.BindProperty(packageProp);
            var locationField = new TextField("Location") {
                enabledSelf = false,
            };
            locationField.BindProperty(locationProp);
            var expanded = false;
            arrow.RegisterCallback<MouseDownEvent>(
                _ => {
                    expanded = !expanded;
                    arrow.text = expanded ? "▼" : "▶";
                    content.style.display = expanded ? DisplayStyle.Flex : DisplayStyle.None;
                }
            );
            cacheField.RegisterValueChangedCallback(
                evt => {
                    if (evt.previousValue == evt.newValue) {
                        return;
                    }
                    if (evt.newValue == null) {
                        locationField.value = string.Empty;
                    } else if (_pkgToAddress[packageField.value].Contains(evt.newValue.name)) {
                        locationField.value = evt.newValue.name;
                    } else {
                        Debug.LogError($"[Object: {evt.newValue}] is not asset of [package: {packageField.value}].");
                        locationField.value = string.Empty;
                        cacheField.value = null;
                    }
                }
            );
            packageField.RegisterValueChangedCallback(
                evt => {
                    if (evt.previousValue == evt.newValue) {
                        return;
                    }
                    locationField.value = string.Empty;
                    cacheField.value = null;
                }
            );
            row.Add(arrow);
            row.Add(cacheField);
            content.Add(packageField);
            content.Add(locationField);
            root.Add(row);
            root.Add(content);
            return root;
        }

        protected static void OpenResourcePicker(UIObjectField objectField, string packageName) {
            if (!_pkgToAssetInfo.TryGetValue(packageName, out var assetInfos) || (assetInfos.Count == 0)) {
                Debug.LogWarning($"[Package: {packageName}] 没有可寻址资源");
                return;
            }
            var assetPaths = new List<string>();
            foreach (var assetInfo in assetInfos) {
                if (assetInfo.AssetType == objectField.objectType) {
                    assetPaths.Add(assetInfo.AssetPath);
                }
            }
            ObjectSelectorWindow.Show(objectField, assetPaths);
        }

        private static void BuildCache() {
            _pkgToAddress.Clear();
            _pkgToAssetInfo.Clear();
            _packages.Clear();
            var setting = AssetBundleCollectorSettingData.Setting;
            foreach (var pkg in setting.Packages) {
                if (!pkg.EnableAddressable) {
                    pkg.EnableAddressable = true;
                    Debug.LogWarning($"[YooAssetReference] 当前包【{pkg.PackageName}】未开启 EnableAddressable，已强制开启。YooAssetReference<T> 仅支持可寻址资源，请确保所有包 EnableAddressable = true。");
                }
                foreach (var group in pkg.Groups) {
                    for (var i = 0; i < group.Collectors.Count; i++) {
                        var collector = group.Collectors[i];
                        if (collector.AddressRuleName is not nameof(AddressByFileName)) {
                            collector.AddressRuleName = nameof(AddressByFileName);
                            Debug.LogWarning($"[YooAssetReference] 当前收集器【Group: {group.GroupName}, Collector Index: {i}】的寻址规则不是 {nameof(AddressByFileName)}，已强制修改。YooAssetReference<T> 要求所有收集器的寻址规则必须为 {nameof(AddressByFileName)}，否则无法正确匹配资源。");
                        }
                    }
                }
                _packages.Add(pkg.PackageName);
                var ignoreRule = AssetBundleCollectorSettingData.GetIgnoreRuleInstance(pkg.IgnoreRuleName);
                var cmd = new CollectCommand(pkg.PackageName, ignoreRule) {
                    SimulateBuild = true,
                    UniqueBundleName = setting.UniqueBundleName,
                    UseAssetDependencyDB = true,
                    EnableAddressable = pkg.EnableAddressable,
                    LocationToLower = pkg.LocationToLower,
                    IncludeAssetGUID = pkg.IncludeAssetGUID,
                    AutoCollectShaders = pkg.AutoCollectShaders,
                };
                var assets = pkg.GetCollectAssets(cmd);
                var hashSet = new HashSet<string>(assets.Count);
                var assetInfos = new List<AssetInfo>(assets.Count);
                _pkgToAddress[pkg.PackageName] = hashSet;
                _pkgToAssetInfo[pkg.PackageName] = assetInfos;
                foreach (var info in assets) {
                    hashSet.Add(info.Address);
                    assetInfos.Add(info.AssetInfo);
                }
            }
        }

    }

}
#endif
