using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace BrunoMikoski.AnimationSequencer {
    // Created by Pablo Huaxteco
    public static class IconLoader {
        public static Texture2D LoadIcon(string iconName) {
            var iconPath = $"{GetEditorPath()}/Icons/{iconName}.png";

            return AssetDatabase.LoadAssetAtPath<Texture2D>(iconPath);
        }

        public static string GetEditorPath() {
            var assemblyName = "BrunoMikoski.AnimationSequencer.Editor";
            var assemblyPath = CompilationPipeline.GetAssemblyDefinitionFilePathFromAssemblyName(assemblyName);
            var directoryPath = System.IO.Path.GetDirectoryName(assemblyPath);
            return directoryPath.Replace("\\", "/");
        }
    }
}