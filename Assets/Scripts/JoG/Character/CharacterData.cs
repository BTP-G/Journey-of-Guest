using EditorAttributes;
using Xoderony.Localization;
using JoG.Localization;
using Unity.Netcode;
using UnityEngine;

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
