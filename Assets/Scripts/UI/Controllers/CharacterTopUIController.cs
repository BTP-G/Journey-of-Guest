using GuestUnion;
using JoG.Character;
using JoG.Messages;
using JoG.UI.Buff;
using JoG.UI.HealthBars;
using MessagePipe;
using System;
using UnityEngine;
using VContainer;

namespace JoG.UI.Controllers {

    public class CharacterTopUIController : MonoBehaviour, IMessageHandler<CharacterBodyChangedMessage> {
        private BuffIconPanel _buffIconPanel;
        private CanvasGroup _canvasGroup;
        private CharacterModel _model;
        private HealthBar _healthBar;
        private CharacterBody _character;
        private Camera _mainCamera;
        private Camera _uiCamera;
        private Canvas _canvas;
        private Transform _uiRoot;
        private IDisposable _disposable;

        void IMessageHandler<CharacterBodyChangedMessage>.Handle(CharacterBodyChangedMessage message) {
            var next = message.next;
            if (next != null && !next.IsLocalPlayer) {
                _canvas.gameObject.SetActive(true);
                _character = next;
                _model = next.Model;
                enabled = true;
            } else {
                _model = null;
                _character = null;
                _canvas.gameObject.SetActive(false);
                enabled = false;
            }
        }

        [Inject]
        internal void Construct(IBufferedSubscriber<CharacterBodyChangedMessage> subscriber) {
            _disposable = subscriber.Subscribe(this);
        }

        protected void Awake() {
            _canvas = GetComponentInChildren<Canvas>();
            _canvasGroup = _canvas.GetComponent<CanvasGroup>();
            _healthBar = GetComponentInChildren<HealthBar>();
            _buffIconPanel = GetComponentInChildren<BuffIconPanel>();
            _mainCamera = Camera.main;
            if (_canvas.worldCamera) {
                _uiCamera = _canvas.worldCamera;
            } else {
                _uiCamera = Camera.allCameras.Find(c => c.name is "UICamera");
                _canvas.worldCamera = _uiCamera;
            }
            _uiRoot = _canvas.transform.GetChild(0);
        }

        protected void Update() {
            if (_model.IsMainRendererVisible) {
                _canvasGroup.alpha = 1;
                var screenPoint = _mainCamera.WorldToScreenPoint(_character.Top);
                _uiRoot.position = _uiCamera.ScreenToWorldPoint(screenPoint);
                _healthBar.UpdateView(_character.PercentHp);
                if ((Time.frameCount & 0b11) is 0b11) {
                    _buffIconPanel.UpdateView(_character.Buffs, _character.ActiveBuffCount);
                }
            } else {
                _canvasGroup.alpha = 0;
            }
        }

        protected void OnDestroy() {
            _disposable?.Dispose();
        }
    }
}