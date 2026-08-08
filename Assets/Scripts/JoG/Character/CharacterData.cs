using EditorAttributes;
using Unity.Netcode;
using UnityEngine;
using Xoderony.Localization;

namespace JoG.Character {

    [CreateAssetMenu(fileName = "CharacterData", menuName = "JoG/CharacterData")]
    public class CharacterData : ScriptableObject {

        [LocalizationKey(@"^character\..*\.name$")]
        public string nameKey = string.Empty;

        [Required, AssetPreview]
        public Sprite iconSprite;

        [Required]
        public NetworkObject networkPrefab;
    }
}
