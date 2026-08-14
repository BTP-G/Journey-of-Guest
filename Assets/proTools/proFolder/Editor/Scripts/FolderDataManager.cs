using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace proTools.proFolder {
    public static class FolderDataManager {
        public static Dictionary<string, Color> folderColors = new Dictionary<string, Color>();
        public static Dictionary<string, int> folderMarkers = new Dictionary<string, int>();

        private static string GetDataPath() {
            return "Assets/proTools/proFolder/proFolderData.json";
        }

        private static MarkerLibrary markerLibrary;

        private static void LoadMarkerLibrary() {
            if (markerLibrary == null) {
                markerLibrary = AssetDatabase.LoadAssetAtPath<MarkerLibrary>("Assets/proTools/proFolder/SO/MarkerLibrary.asset");
            }
        }

        public static void Save() {
            var dataList = new FolderDataList();

            foreach (var kvp in folderColors) {
                folderMarkers.TryGetValue(kvp.Key, out var markerIndex);
                dataList.folders.Add(new FolderData {
                    path = kvp.Key,
                    color = kvp.Value,
                    markerIndex = markerIndex
                });
            }

            var json = JsonUtility.ToJson(dataList, true);
            File.WriteAllText(GetDataPath(), json);
            AssetDatabase.Refresh();
        }

        public static void Load() {
            if (!File.Exists(GetDataPath())) {
                return;
            }

            var json = File.ReadAllText(GetDataPath());
            var dataList = JsonUtility.FromJson<FolderDataList>(json);

            folderColors.Clear();
            folderMarkers.Clear();

            foreach (var data in dataList.folders) {
                folderColors[data.path] = data.color;
                folderMarkers[data.path] = data.markerIndex;
            }
        }

        public static void AutoAssignMarkers() {
            LoadMarkerLibrary();

            var allFolders = new List<string>();
            GetAllFoldersRecursively("Assets", allFolders);

            foreach (var folder in allFolders) {
                var files = Directory.GetFiles(folder, "*", SearchOption.AllDirectories);

                var markerIndex = DetermineMarkerByFiles(files);
                if (markerIndex >= 0) {
                    folderMarkers[folder] = markerIndex;
                }
            }

            Save();
        }

        private static void GetAllFoldersRecursively(string root, List<string> folders) {
            folders.Add(root);

            var subFolders = AssetDatabase.GetSubFolders(root);
            foreach (var sub in subFolders) {
                GetAllFoldersRecursively(sub, folders);
            }
        }

        private static int DetermineMarkerByFiles(string[] files) {
            LoadMarkerLibrary();

            Dictionary<int, int> markerCounts = new();

            foreach (var file in files) {
                var ext = Path.GetExtension(file).ToLower();

                for (var i = 0; i < markerLibrary.entries.Count; i++) {
                    var entry = markerLibrary.entries[i];
                    if (entry.fileExtensions.Contains(ext)) {
                        if (!markerCounts.ContainsKey(i)) {
                            markerCounts[i] = 0;
                        }

                        markerCounts[i]++;
                        break;
                    }
                }
            }

            if (markerCounts.Count == 0) {
                return -1;
            }

            return markerCounts.OrderByDescending(kvp => kvp.Value).First().Key;
        }
    }
}
