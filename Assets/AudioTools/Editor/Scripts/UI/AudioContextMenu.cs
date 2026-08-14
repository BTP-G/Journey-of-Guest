using System.IO;
using UnityEditor;
using UnityEngine;

namespace AudioTool {
    /// <summary>
    /// Provides context menu integration for editing audio files directly from the Project window.
    /// </summary>
    public static class AudioContextMenu {
        private static readonly string[] SupportedExtensions = { ".wav", ".mp3", ".ogg", ".aiff", ".aif", ".flac" };

        [MenuItem("Assets/Edit Audio...", false, 200)]
        private static void EditAudio() {
            var selectedObjects = Selection.objects;

            foreach (var obj in selectedObjects) {
                var assetPath = AssetDatabase.GetAssetPath(obj);
                if (string.IsNullOrEmpty(assetPath)) {
                    continue;
                }

                if (IsAudioFile(assetPath)) {
                    var fullPath = Path.Combine(Path.GetDirectoryName(Application.dataPath), assetPath);
                    var source = new FileAudioSource(fullPath);

                    var window = AudioEditorUI.ShowWindow();
                    window.Init(source, Path.GetDirectoryName(fullPath));
                }
            }
        }

        [MenuItem("Assets/Edit Audio...", true)]
        private static bool ValidateEditAudio() {
            var selectedObjects = Selection.objects;
            if (selectedObjects == null || selectedObjects.Length != 1) {
                return false;
            }

            foreach (var obj in selectedObjects) {
                var assetPath = AssetDatabase.GetAssetPath(obj);
                if (!string.IsNullOrEmpty(assetPath) && IsAudioFile(assetPath)) {
                    return true;
                }
            }

            return false;
        }

        private static bool IsAudioFile(string path) {
            if (string.IsNullOrEmpty(path)) {
                return false;
            }

            var extension = Path.GetExtension(path).ToLowerInvariant();
            foreach (var ext in SupportedExtensions) {
                if (extension == ext) {
                    return true;
                }
            }

            return false;
        }
    }
}
