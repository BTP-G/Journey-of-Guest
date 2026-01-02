using GuestUnion.ObjectPool.Generic;
using UnityEditor;
using UnityEngine;

namespace JoG.Editor {

    public static class MissingScriptFinder {

        [MenuItem("Tools/Check All Prefabs For Missing Scripts")]
        public static void CheckAllPrefabs() {
            // 收集所有 prefab 路径
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
            if (guids.Length == 0) {
                Debug.Log("No prefabs found.");
                return;
            }

            int missingCount = 0;
            for (int i = 0; i < guids.Length; i++) {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                bool cancel = EditorUtility.DisplayCancelableProgressBar(
                    "Checking prefabs for missing scripts...",
                    $"{i + 1}/{guids.Length}  {path}",
                    (float)i / guids.Length);
                if (cancel) break;

                GameObject prefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (!prefabRoot) continue;

                // 深度遍历所有子节点
                foreach (Transform tr in prefabRoot.GetComponentsInChildren<Transform>(true)) {
                    Component[] components = tr.GetComponents<Component>();
                    for (int c = 0; c < components.Length; c++) {
                        if (components[c] == null)    // 这就是 Missing Script
                        {
                            missingCount++;
                            Debug.LogError(
                                $"Missing script detected!  " +
                                $"Prefab: {path}  GameObject: {GetFullPath(tr)}",
                                prefabRoot);
                            // 发现一处就跳出内层循环，继续检查下一个物体
                            break;
                        }
                    }
                }
            }

            EditorUtility.ClearProgressBar();
            Debug.Log($"<b>Missing script check complete.</b>  Total missing: {missingCount}");
        }

        [MenuItem("Tools/Find Missing Scripts")]
        private static void FindMissing() {
            using (ListPool<Component>.Rent(out var buffer)) {
                foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None)) {
                    go.GetComponents(buffer);
                    foreach (var component in buffer) {
                        if (component == null) {
                            Debug.Log($"Missing script on: {go.name}", go);
                        }
                    }
                }
            }
        }

        // 构造类似 "/Root/Child/GrandChild" 的完整路径
        private static string GetFullPath(Transform tr) {
            string s = tr.name;
            while (tr.parent != null) {
                tr = tr.parent;
                s = $"{tr.name}/{s}";
            }
            return $"/{s}";
        }
    }
}