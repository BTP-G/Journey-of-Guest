using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BrunoMikoski.AnimationSequencer {
    public class EditorDefaultResourceSingleton<T> : ScriptableObject
    where T : ScriptableObject {
        private static T instance;
        public static T Instance {
            get {
                if (instance == null) {
                    instance = LoadOrCreateInstance();
                }

                return instance;
            }
        }

        private static T LoadOrCreateInstance() {
            if (!TryToLoadInstance(out var resultInstance)) {
                resultInstance = CreateInstance<T>();

                var absolutePath = Path.GetFullPath("Assets/Editor Default Resources");

                if (!Directory.Exists(absolutePath)) {
                    Directory.CreateDirectory(absolutePath);
                }

                AssetDatabase.Refresh();
                AssetDatabase.CreateAsset(resultInstance, $"Assets/Editor Default Resources/{typeof(T).Name}.asset");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                return resultInstance;
            }

            return resultInstance;
        }

        public static bool Exist() {
            return TryToLoadInstance(out _);
        }

        private static bool TryToLoadInstance(out T result) {
            var registryGUID = AssetDatabase.FindAssets($"t:{typeof(T).Name}")
                .FirstOrDefault();
            if (!string.IsNullOrEmpty(registryGUID)) {
                var instance = (T)AssetDatabase.LoadAssetAtPath<ScriptableObject>(
                     AssetDatabase.GUIDToAssetPath(registryGUID)
                 );
                if (instance != null) {
                    result = instance;
                    return true;
                }
            }
            result = null;
            return false;
        }
    }
}
