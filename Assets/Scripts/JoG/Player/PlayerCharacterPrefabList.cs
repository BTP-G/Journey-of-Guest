using EditorAttributes;
using Xoderony.Localization;
using JoG.Localization;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace JoG.Player {

    [Serializable]
    public struct PlayerCharacterPrefabCard {

        [LocalizationKey(@"^character\..*\.name$")]
        public string nameKey;

        [Required]
        public Sprite icon;

        [Required]
        public NetworkObject networkPrefab;
    }

    [CreateAssetMenu(fileName = "PlayerCharacterPrefabList", menuName = "JoG/Player Prefab List")]
    public class PlayerCharacterPrefabList : ScriptableObject {
        public List<PlayerCharacterPrefabCard> prefabs = new();
    }
}
