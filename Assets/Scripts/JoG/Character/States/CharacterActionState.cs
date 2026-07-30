using Animancer;
using JoG.States;
using UnityEngine;

namespace JoG.Character.States {

    public class CharacterActionState : CharacterAnimancerState {
        public static readonly StringReference ActionEventName = "Action";

        [SerializeField]
        private ClipTransition _animation = new();

        [SerializeField, Min(0)]
        private int _layerIndex = 1;

        [SerializeField]
        private AvatarMask _mask;

        [SerializeField, Min(0)]
        private float _fadeOutDuration = AnimancerGraph.DefaultFadeDuration;

        private AnimancerState _animationState;

        public event System.Action Completed;

        public Vector3 Direction { get; set; }

        protected void OnEnable() {
            var layer = animancer.Layers[_layerIndex];
            if (_mask != null) {
                layer.Mask = _mask;
            }

            _animationState = layer.Play(_animation);
            var events = _animationState.Events(this);
            events.SetCallbacks(ActionEventName, OnAction);
            events.OnEnd = OnAnimationEnd;
        }

        protected void OnDisable() {
            if (_animationState != null) {
                var events = _animationState.Events(this);
                events.SetCallbacks(ActionEventName, AnimancerEvent.DummyCallback);
                events.OnEnd = null;
                _animationState = null;
            }

            if (_layerIndex > 0) {
                animancer.Layers[_layerIndex].StartFade(0, _fadeOutDuration);
            }
        }

        protected virtual void OnAction() { }

        private void OnAnimationEnd() {
            Completed?.Invoke();
        }
    }

}
