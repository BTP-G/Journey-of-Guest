using GuestUnion;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using YooAsset;

namespace JoG.InventorySystem {

    public static class ItemCollector {
        private static readonly Dictionary<string, ItemData> _nameToItemDatas = new();
        public static IReadOnlyDictionary<string, ItemData> AllItems => _nameToItemDatas;

        /// <summary>单个注册ItemData</summary>
        public static void Register(ItemData item) {
            if (item == null) {
                Debug.LogError("Attempted to register a null ItemData.");
                return;
            }
            if (item.nameToken.IsNullOrEmpty()) {
                Debug.LogError($"ItemData '{item.name}' has no nameToken set. Cannot register.");
                return;
            }
            ref var i = ref _nameToItemDatas.GetValueRefOrAddDefault(item.nameToken , out var exists);
            if (exists) {
                Debug.LogWarning($"ItemData '{item.nameToken}' is already registered. Overwriting existing entry.");
            }
            i = item;
            Debug.Log($"Registered ItemData: {item.nameToken}");
        }

        /// <summary>批量注册：通过YooAsset资源包和标签加载所有ItemData</summary>
        public static void RegisterFromPackage(ResourcePackage package) {
            if (package == null) throw new ArgumentNullException(nameof(package));
            foreach (var assetInfo in package.GetAssetInfos("item").AsSpan()) {
                var op = package.LoadAssetSync(assetInfo);
                if (op.Status == EOperationStatus.Succeed) {
                    if (op.AssetObject is ItemData itemData) {
                        Register(itemData);
                    } else {
                        Debug.LogWarning($"[Asset: {op.AssetObject}] '{assetInfo.AssetPath}' is not of type ItemData. Skipping registration.");
                    }
                }
            }
        }

        /// <summary>查询</summary>
        public static bool TryGetItemDef(string itemName, out ItemData itemData)
            => _nameToItemDatas.TryGetValue(itemName, out itemData);

        /// <summary>清空所有已注册</summary>
        public static void Clear() => _nameToItemDatas.Clear();
    }
}