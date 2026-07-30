using ANU.IngameDebug.Console;
using Cysharp.Text;
using Xoderony.Logging;
using JoG.Character;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;

[assembly: RegisterDebugCommandTypes(typeof(CharacterDataDictionary))]

namespace JoG.Character {

    [DebugCommandPrefix("character")]
    public class CharacterDataDictionary : IReadOnlyDictionary<string, CharacterData> {
        public static readonly CharacterDataDictionary Shared = new();
        private readonly Dictionary<string, CharacterData> _nameToData = new();
        IEnumerable<string> IReadOnlyDictionary<string, CharacterData>.Keys => _nameToData.Keys;

        IEnumerable<CharacterData> IReadOnlyDictionary<string, CharacterData>.Values => _nameToData.Values;

        public int Count => _nameToData.Count;

        public Dictionary<string, CharacterData>.KeyCollection Keys => _nameToData.Keys;

        public Dictionary<string, CharacterData>.ValueCollection Datas => _nameToData.Values;

        public CharacterData this[string name] => _nameToData[name];

        [DebugCommand]
        public static void PrintDatas() {
            using var sb = ZString.CreateStringBuilder(true);
            foreach (var data in Shared.Datas) {
                sb.AppendLine(JsonConvert.SerializeObject(data, Formatting.None));
            }
            Shared.Log(sb.ToString());
        }

        public void Add(CharacterData data) => _nameToData.Add(data.name, data);

        public bool Remove(CharacterData data) => _nameToData.Remove(data.name);

        public void Clear() => _nameToData.Clear();

        public bool ContainsKey(string name) => _nameToData.ContainsKey(name);

        public bool TryGetValue(string name, out CharacterData data) => _nameToData.TryGetValue(name, out data);

        IEnumerator<KeyValuePair<string, CharacterData>> IEnumerable<KeyValuePair<string, CharacterData>>.GetEnumerator() => _nameToData.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _nameToData.GetEnumerator();
    }
}
