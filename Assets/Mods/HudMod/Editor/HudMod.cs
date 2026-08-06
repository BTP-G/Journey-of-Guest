using System;
using System.Collections.Generic;
using ANU.IngameDebug.Console;
using Xoderony.Logging;
using JoG.Character;
using JoG.Health;
using MessagePipe;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;

namespace JoG.Modding {

    [AddComponentMenu("Mods/HUD Mod")]
    [DebugCommandPrefix("hud.mod")]
    public sealed class HudMod : MonoBehaviour {
        private readonly List<DamageEntry> _entries = new(256);

        [SerializeField] private bool _showOnStart = false;

        private bool _isVisible;

        private int _totalDamage;

        private int _maxHit;

        private float _dps;

        private Entity _player;

        private float _nextPollTime;

        private StatDisplay[] _stats = Array.Empty<StatDisplay>();

        private LifetimeScope _scope;
        private IDisposable _subscription;

        private GUIStyle _titleStyle;

        private GUIStyle _labelStyle;

        private GUIStyle _valueStyle;

        private bool _stylesReady;

        [DebugCommand("log")]
        public void DebugCharacterProperty() {
            var sb = new System.Text.StringBuilder();
            foreach (var entity in Entity.Entities) {
                if (!entity.TryGetComponent<IEnumerable<Stat>>(out var stats)) {
                    continue;
                }
                sb.Clear();
                foreach (var stat in stats) {
                    sb.Append("  ")
                      .Append(stat.Name)
                      .Append(": ")
                      .AppendLine(GetStatValue(stat));
                }
                this.Log($"Entity {entity.Id}:\n{sb}");
            }
        }

        private void Awake() {
            _isVisible = _showOnStart;
        }

        private void OnEnable() {
            SceneManager.sceneLoaded += OnSceneLoaded;
            TrySubscribe();
        }

        private void OnDisable() {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            DisposeSubscription();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
            DisposeSubscription();
            _player = null;
            _stats = Array.Empty<StatDisplay>();
            _entries.Clear();
            _totalDamage = 0;
            _maxHit = 0;
            _dps = 0;
            TrySubscribe();
        }

        private void TrySubscribe() {
            if (_subscription != null) return;
            foreach (var scope in FindObjectsByType<LifetimeScope>(FindObjectsSortMode.None)) {
                try {
                    if (!scope.Container.TryResolve(out ISubscriber<HealthChangeReport> subscriber)) {
                        continue;
                    }
                    _scope = scope;
                    _subscription = subscriber.Subscribe(OnHealthChangeReport);
                    this.Log("Subscribed to HealthChangeReport");
                    return;
                } catch (Exception ex) {
                    Debug.LogWarning($"[HudMod] Subscribe failed in scope '{scope.name}': {ex.Message}");
                }
            }
        }

        private void DisposeSubscription() {
            try {
                _subscription?.Dispose();
            } catch {
            }
            _scope = null;
            _subscription = null;
        }

        private void OnHealthChangeReport(HealthChangeReport report) {
            var damage = Mathf.Max(0, -report.deltaValue);
            if (damage == 0) return;
            this.Log($"HealthChangeReport: {report.source} -> {report.target}, damage: {damage}");
            if (_player == null) return;
            if (report.source != _player) return;

            _entries.Add(new DamageEntry { time = Time.time, damage = damage });
            _totalDamage += damage;
            if (damage > _maxHit) _maxHit = damage;
        }

        private void Update() {
            if (Keyboard.current != null && Keyboard.current.f6Key.wasPressedThisFrame) {
                _isVisible = !_isVisible;
            }

            if (_player == null) {
                TryFindPlayer();
            }

            if (_scope == null) {
                TrySubscribe();
            }

            if (Time.time >= _nextPollTime && _player != null) {
                PollStats();
                _nextPollTime = Time.time + 0.25f;
            }

            UpdateDps();
        }

        private void TryFindPlayer() {
            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsConnectedClient) return;

            var client = NetworkManager.Singleton.LocalClient;
            if (client?.PlayerObject == null) return;

            var newPlayer = client.PlayerObject.GetComponent<Entity>();
            if (newPlayer != null && newPlayer != _player) {
                _player = newPlayer;
                _entries.Clear();
                _totalDamage = 0;
                _maxHit = 0;
                _dps = 0;
            }
        }

        private void PollStats() {
            if (_player == null) return;
            var list = new List<StatDisplay>();
            if (_player.TryGetComponent(out HealthComponent health)) {
                list.Add(new StatDisplay {
                    label = "HP",
                    value = $"{health.Current} / {health.Max}"
                });
            }
            if (_player.TryGetComponent<IEnumerable<Stat>>(out var stats)) {
                foreach (var stat in stats) {
                    list.Add(new StatDisplay {
                        label = stat.Name,
                        value = GetStatValue(stat)
                    });
                }
            }
            _stats = list.ToArray();
        }

        private static string GetStatValue(Stat stat) {
            return stat.Value.ToString();
        }

        private void UpdateDps() {
            var now = Time.time;
            _entries.RemoveAll(e => now - e.time > 5f);

            if (_entries.Count == 0) {
                _dps = 0;
                return;
            }

            float total = 0;
            foreach (var e in _entries) total += e.damage;

            var window = Mathf.Min(5f, now - _entries[0].time);
            _dps = window > 0.001f ? total / window : total;
        }

        private void EnsureStyles() {
            if (_stylesReady) return;
            _stylesReady = true;

            _titleStyle = new GUIStyle(GUI.skin.label) {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.2f, 0.8f, 1f) }
            };
            _labelStyle = new GUIStyle(GUI.skin.label) {
                fontSize = 13,
                normal = { textColor = Color.white }
            };
            _valueStyle = new GUIStyle(GUI.skin.label) {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleRight,
                normal = { textColor = Color.yellow }
            };
        }

        private void OnGUI() {
            if (!_isVisible) return;
            EnsureStyles();

            float x = 10f;
            float y = 10f;
            float w = 270f;
            float lineH = 22f;

            float damageLines = 1 + 3 + 1;
            float playerLines = _stats.Length > 0 ? 1 + _stats.Length + 1 : 0;
            float boxH = (damageLines + playerLines) * lineH + 20f;

            GUI.Box(new Rect(x - 5, y - 5, w + 10, boxH), "");

            GUI.Label(new Rect(x, y, w, lineH), "─ Damage ─", _titleStyle);
            y += lineH + 2;

            DrawEntry(x, y, w, "DPS:", $"{_dps:F1}"); y += lineH;
            DrawEntry(x, y, w, "Total:", $"{_totalDamage:N0}"); y += lineH;
            DrawEntry(x, y, w, "Max Hit:", $"{_maxHit:N0}"); y += lineH + 8;

            if (_stats.Length > 0) {
                GUI.Label(new Rect(x, y, w, lineH), "─ Player ─", _titleStyle);
                y += lineH + 2;
                foreach (var s in _stats) {
                    DrawEntry(x, y, w, s.label, s.value);
                    y += lineH;
                }
            }
        }

        private void DrawEntry(float x, float y, float w, string label, string value) {
            GUI.Label(new Rect(x, y, w * 0.4f, 20), label, _labelStyle);
            GUI.Label(new Rect(x + w * 0.4f, y, w * 0.6f, 20), value, _valueStyle);
        }

        private void OnDestroy() {
            DisposeSubscription();
        }

        private struct DamageEntry { public float time; public int damage; }

        private struct StatDisplay {
            public string label;
            public string value;
        }
    }
}
