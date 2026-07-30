using ANU.IngameDebug.Console;
using Cysharp.Text;
using Xoderony.Logging;
using JoG.Item;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;

[assembly: RegisterDebugCommandTypes(typeof(ItemDataDictionary))]

namespace JoG.Item {

    [DebugCommandPrefix("item")]
    public class ItemDataDictionary : IReadOnlyDictionary<string, ItemData> {
        public static readonly ItemDataDictionary Shared = new();
        private readonly Dictionary<string, ItemData> _nameToData = new();
        IEnumerable<string> IReadOnlyDictionary<string, ItemData>.Keys => _nameToData.Keys;

        IEnumerable<ItemData> IReadOnlyDictionary<string, ItemData>.Values => _nameToData.Values;

        public int Count => _nameToData.Count;

        public Dictionary<string, ItemData>.KeyCollection Keys => _nameToData.Keys;

        public Dictionary<string, ItemData>.ValueCollection Datas => _nameToData.Values;

        public ItemData this[string name] => _nameToData[name];

        [DebugCommand]
        public static void PrintDatas() {
            using var sb = ZString.CreateStringBuilder(true);
            foreach (var data in Shared.Datas) {
                sb.AppendLine(JsonConvert.SerializeObject(data, Formatting.None));
            }
            Shared.Log(sb.ToString());
        }

        public void Add(ItemData data) => _nameToData.Add(data.name, data);

        public bool Remove(ItemData data) => _nameToData.Remove(data.name);

        public void Clear() => _nameToData.Clear();

        public bool ContainsKey(string name) => _nameToData.ContainsKey(name);

        public bool TryGetValue(string name, out ItemData data) => _nameToData.TryGetValue(name, out data);

        IEnumerator<KeyValuePair<string, ItemData>> IEnumerable<KeyValuePair<string, ItemData>>.GetEnumerator() => _nameToData.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _nameToData.GetEnumerator();
    }
}
