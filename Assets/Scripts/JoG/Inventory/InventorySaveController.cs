using Cysharp.Threading.Tasks;
using EditorAttributes;
using Xoderony.Logging;
using JoG.Item;
using JoG.Networking;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VContainer;

namespace JoG.Inventory {

    public class InventorySaveController : MonoBehaviour, IComponent, INetworkSpawnHandler, INetworkDespawnHandler {
        [Inject] internal ISessionService sessionService;
        [Required, SerializeField] private NetworkInventory _inventory;
        private Dictionary<string, int> _nameToCount;
        [ReadOnly, SerializeField] private string _filePath;
        private float _lastSaveTime;

        private bool _isQuitting = false;
        private bool _isSaving = false;

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
                            _inventory.AddItemRpc(itemData, slotData.Value);
                        }
                    }
                } catch (Exception ex) {
                    this.LogError($"Failed to load inventory: {ex}");
                }
            } else {
                _nameToCount = new Dictionary<string, int>();
            }
            _inventory.OnItemCountChanged += OnInventoryChanged;
        }

        private void OnDisable() {
            _inventory.OnItemCountChanged -= OnInventoryChanged;
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

        private async UniTask DebouncedSaveAsync() {
            const float saveDelay = 1.0f;
            _lastSaveTime = Time.unscaledTime;
            if (_isSaving) return;
            _isSaving = true;
            var delayTimeSpan = TimeSpan.FromSeconds(saveDelay);
            do {
                await UniTask.Delay(delayTimeSpan);
            } while (Time.unscaledTime - _lastSaveTime <= saveDelay);
            SaveInventoryImmediate();
            _isSaving = false;
        }
    }
}
