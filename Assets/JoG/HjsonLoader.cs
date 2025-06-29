using Hjson;
using JoG.ExtensionMethods;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace JoG {

    public static class HjsonLoader {

        public static Dictionary<string, JsonValue> LoadConfig() {
            var path = Path.Combine(Application.streamingAssetsPath, "config.hjson");
            return LoadHjsonAsDictionary(path);
        }

        public static Dictionary<string, JsonValue> LoadLocalization(string language) {
            var path = Path.Combine(Application.streamingAssetsPath, $"Localization/{language}.hjson");
            return LoadHjsonAsDictionary(path);
        }

        private static Dictionary<string, JsonValue> LoadHjsonAsDictionary(string path) {
            if (!File.Exists(path)) {
                Debug.LogError($"File not found: {path}");
                return null;
            }
            var jv = HjsonValue.Load(path);
            if (jv is not JsonObject) {
                Debug.LogError($"File at {path} is not a valid JSON object.");
                return null;
            }
            var dict = new Dictionary<string, JsonValue>(100);
            jv.FlattenJson(string.Empty, dict);
            return dict;
        }
    }
}