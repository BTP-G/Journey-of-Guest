using EditorAttributes;
using JoG.Health;
using JoG.UI.Buff;
using JoG.UI.Health;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;
using VContainer;
using Xoderony;
using Xoderony.Localization;
using Xoderony.Unity;

namespace JoG.Character {

    [DisallowMultipleComponent]
    [RequireComponent(typeof(Canvas))]
    public class CharacterNameplate : MonoBehaviour, IComponent {

        [LocalizationKey(@"^character\..*\.name$")]
        public string nameKey;

        [Inject] internal CharacterEffects _effects;
        [Inject] internal HealthComponent _health;
        [Inject] internal Billboarder billboarder;
        [SerializeField, Required] private TextMeshProUGUI _nameText;
        [SerializeField] private WorldHealthBar _healthBar;
        [SerializeField] private WorldBuffBar _buffBar;
        private Canvas canvas;

        public string CharacterName {
            get => _nameText.text;
            set => _nameText.text = value;
        }

        [Inject]
        internal void Inject(
            IDelegateSubscriber<CharacterLifeStartHandler> lifeStarted,
            IDelegateSubscriber<CharacterLifeStopHandler> lifeStopped) {

            lifeStarted.Subscribe(_ => {
                canvas.enabled = true;
            });
            lifeStopped.Subscribe(_ => {
                canvas.enabled = false;
            });
        }

        protected void Awake() {
            canvas = GetComponent<Canvas>();
            var mainCamera = Camera.main;
            canvas.worldCamera = mainCamera;
            Localizer.OnLanguageUpdated += OnLanguageUpdated;
        }

        protected void OnDestroy() {
            Localizer.OnLanguageUpdated -= OnLanguageUpdated;
        }

        protected void OnEnable() {
            PostUpdateLoop<PreLateUpdate.ScriptRunBehaviourLateUpdate>.Register(OnPostLateUpdate);
            billboarder.Register(transform);
        }

        protected void OnPostLateUpdate() {
            _healthBar.UpdateView(_health.Ratio);
            if ((Time.frameCount & 0b11) == 0b11) {
                _buffBar.UpdateView(_effects);
            }
        }

        protected void OnDisable() {
            PostUpdateLoop<PreLateUpdate.ScriptRunBehaviourLateUpdate>.Unregister(OnPostLateUpdate);
            billboarder.Unregister(transform);
        }

        private void OnLanguageUpdated() {
            CharacterName = Localizer.GetString(nameKey);
        }
    }
}
