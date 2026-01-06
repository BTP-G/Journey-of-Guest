using System.IO;
using YooAsset.Editor;

namespace GuestUnion.YooAsset.Editor {

    [DisplayName("收集Prefabs目录下的预制体")]
    public class CollectPrefabsPrefab : IFilterRule {
        public string FindAssetType => EAssetSearchType.Prefab.ToString();

        public bool IsCollectAsset(FilterRuleData data) {
            return data.AssetPath.Contains("/Prefabs/") && Path.GetExtension(data.AssetPath) == (".prefab");
        }
    }
}