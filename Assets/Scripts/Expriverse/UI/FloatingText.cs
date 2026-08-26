using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using VContainer;
using Xoderony.Unity;

namespace Expriverse.UI {

    [DisallowMultipleComponent]
    [RequireComponent(typeof(TextMeshPro))]
    public class FloatingText : MonoBehaviour {
        [Inject, SerializeField, HideInInspector] internal Billboarder billboarder;
        private TextMeshPro _textMesh;
        private Action<FloatingText> _stopAction;

        public TextMeshPro TextMesh => _textMesh;

        public FloatingText Position(in Vector3 position) {
            transform.position = position;
            return this;
        }

        public FloatingText Text(string text) {
            _textMesh.text = text;
            _textMesh.rectTransform.sizeDelta = _textMesh.GetPreferredValues();
            return this;
        }

        public FloatingText Color(in Color color) {
            _textMesh.color = color;
            return this;
        }

        public void Fire(in Vector3 displacement, float duration = 3f, Action<FloatingText> stopAction = null) {
            _stopAction = stopAction;
            gameObject.layer = 5;
            billboarder.Register(transform);
            transform.DOScale(1f, 0.2f)
                .From(0.5f)
                .SetEase(Ease.OutBack);
            transform.DOMove(transform.position + displacement, duration)
                .SetEase(Ease.OutQuad)
                .OnComplete(OnStop);
        }

        private void OnStop() {
            billboarder.Unregister(transform);
            if (_stopAction == null) {
                Destroy(gameObject);
                return;
            }
            gameObject.layer = 31;
            _stopAction.Invoke(this);
            _stopAction = null;
        }

        private void Awake() {
            _textMesh = GetComponent<TextMeshPro>();
            gameObject.layer = 31;
        }

        private void OnDestroy() {
            billboarder.Unregister(transform);
            transform.DOKill();
        }
    }
}
