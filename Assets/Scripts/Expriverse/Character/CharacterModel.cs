using UnityEngine;
using VContainer;
using Xoderony;

namespace Expriverse.Character {

    [DisallowMultipleComponent]
    [RequireComponent(typeof(Renderer))]
    [RequireComponent(typeof(CapsuleCollider))]
    public class CharacterModel : MonoBehaviour, IComponent {
        private Transform _transform;
        private Vector3 _nameplateOffset;
        private Vector3 _topOffset;
        private Vector3 _centerOffset;
        private Vector3 _bottomOffset;
        private IDelegateDispatcher<VisibleChangeHandler> _visibleChangedHandlers;

        public Renderer MainRenderer { get; private set; }
        public CapsuleCollider MainCollider { get; private set; }
        public Vector3 NameplatePosition => _transform.position + _nameplateOffset;
        public Vector3 Top => _transform.position + _topOffset;
        public Vector3 Center => _transform.position + _centerOffset;
        public Vector3 Bottom => _transform.position + _bottomOffset;
        public float BoundsRadius { get; private set; }
        public float Height { get; private set; }

        [Inject]
        internal void Inject(IDelegateDispatcher<VisibleChangeHandler> visibleChangedHandlers) {
            _visibleChangedHandlers = visibleChangedHandlers;
        }

        private void Awake() {
            _transform = transform;
            MainRenderer = GetComponent<Renderer>();
            MainCollider = GetComponent<CapsuleCollider>();
            var size = MainCollider.bounds.size;
            Height = size.y;
            BoundsRadius = Mathf.Max(size.x, size.y) * 0.5f;
            _centerOffset = MainCollider.center;
            _nameplateOffset = _centerOffset + new Vector3(0, 1.2f * BoundsRadius, 0);
            _topOffset = _centerOffset + new Vector3(0, BoundsRadius, 0);
            _bottomOffset = _centerOffset - new Vector3(0, BoundsRadius, 0);
        }

        private void OnBecameVisible() {
            _visibleChangedHandlers.Handlers?.Invoke(true);
        }

        private void OnBecameInvisible() {
            _visibleChangedHandlers.Handlers?.Invoke(false);
        }
    }
}
