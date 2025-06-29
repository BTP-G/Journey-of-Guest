using Hjson;
using System.Collections.Generic;

namespace JoG.ExtensionMethods {

    public static class JsonObjectExtensions {
        
        public static void FlattenJson(this JsonValue value, string prefix, Dictionary<string, JsonValue> dict) {
            dict[prefix] = value;
            if (value is JsonObject jo) {
                foreach (var kv in jo) {
                    var newPrefix = string.IsNullOrEmpty(prefix) ? kv.Key : $"{prefix}.{kv.Key}";
                    FlattenJson(kv.Value, newPrefix, dict);
                }
            }
        }

        public static void FlattenJson(this JsonValue value, string prefix, Dictionary<string, string> dict) {
            if (value is JsonObject jo) {
                foreach (var kv in jo) {
                    var newPrefix = string.IsNullOrEmpty(kv.Key) ? kv.Key : $"{kv.Key}";
                    FlattenJson(kv.Value, newPrefix, dict);
                }
            } else if (value.JsonType is JsonType.String) {
                dict[prefix] = value;
            }
        }
    }
}