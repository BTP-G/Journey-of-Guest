using EditorAttributes;
using JoG.Item;
using JoG.Networking;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VContainer;
using Xoderony.Logging;

namespace JoG.Inventory {

    public class InventorySaveController : MonoBehaviour, IComponent, INetworkSpawnHandler, INetworkDespawnHandler {
        [Inject] internal ISessionService sessionService;
        [Inject] internal CharacterInventory inventory;
        private Dictionary<string, int> _nameToCount;
        [ReadOnly, SerializeField] private string _filePath;

        private bool _isQuitting = false;

        public void SaveInventoryImmediate() {
            try {
                var json = JsonConvert.SerializeObject(_nameToCount);
                File.WriteAllText(_filePath, json);
            } catch (Exception ex) {
                this.LogError($"Failed to save inventory immediately: {ex}");
            }
        }

        void INetworkSpawnHandler.OnSpawn(bool isOwner) {
            enabled = isOwner;
        }

        void INetworkDespawnHandler.OnDespawn(bool isOwner) {
            enabled = false;
        }

        private void OnEnable() {
            var directoryPath = Path.Combine(Application.persistentDataPath, "InventorySaves");
            _filePath = Path.Combine(directoryPath, $"{sessionService.Session.Code}.json");
            Directory.CreateDirectory(directoryPath);
            var path = _filePath;
            if (File.Exists(path)) {
                try {
                    var json = File.ReadAllText(path);
                    _nameToCount = JsonConvert.DeserializeObject<Dictionary<string, int>>(json) ?? new();
                    foreach (var slotData in _nameToCount) {
                        if (ItemDataDictionary.Shared.TryGetValue(slotData.Key, out var itemData)) {
                            inventory.AddItem(itemData, slotData.Value);
                        }
                    }
                } catch (Exception ex) {
                    this.LogError($"Failed to load inventory: {ex}");
                }
            } else {
                _nameToCount = new Dictionary<string, int>();
            }
            inventory.ItemCountChanged += OnInventoryChanged;
        }

        private void OnDisable() {
            inventory.ItemCountChanged -= OnInventoryChanged;
            if (!_isQuitting) {
                SaveInventoryImmediate();
            }
        }

        private void OnValidate() {
            enabled = false;
        }

        private void OnApplicationQuit() {
            _isQuitting = true;
            SaveInventoryImmediate();
        }

        private void OnInventoryChanged(ItemData item, int count) {
            if (count == 0) {
                _nameToCount.Remove(item.name);
            } else {
                _nameToCount[item.name] = count;
            }
            SaveInventoryImmediate();
        }
    }
}

