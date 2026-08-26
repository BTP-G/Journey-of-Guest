using System;
using UnityEngine;
using UnityEngine.Events;

namespace Expriverse.Character.States.Fighter {

    public class FighterAnimationEventDispatcher : MonoBehaviour, IComponent {
        public AnimationEvent2 onSwordSwing = new();

        public AnimationEvent2 onShieldSwing = new();

        public void OnSwordSwing(AnimationEvent animationEvent) {
            onSwordSwing.Invoke(animationEvent);
        }

        public void OnShieldSwing(AnimationEvent animationEvent) {
            onShieldSwing.Invoke(animationEvent);
        }

        [Serializable]
        public class AnimationEvent2 : UnityEvent2<AnimationEvent> { }
    }
}
