using UnityEngine;
using VContainer;
using Xoderony;

namespace JoG {

    [RequireComponent(typeof(Animator))]
    public class AnimationEventDispatcher : MonoBehaviour, IComponent {
        private IDelegateDispatcher<AnimationEventHandler> _animationEventHandlers;

        [Inject]
        internal void Inject(IDelegateDispatcher<AnimationEventHandler> animationEventHandlers) {
            _animationEventHandlers = animationEventHandlers;
        }

        private void HandleAnimationEvent(AnimationEvent animationEvent) {
            _animationEventHandlers.Handlers?.Invoke(animationEvent);
        }
    }
}
