using System;
using UnityEngine;
using UnityEngine.Events;

namespace Expriverse.Character.States.Golem {

    public class GolemAnimationEventDispatcher : MonoBehaviour, IComponent {
        public AnimationEvent2 onStomp = new();

        public void Stomp(AnimationEvent animationEvent) {
            onStomp.Invoke(animationEvent);
        }

        [Serializable]
        public class AnimationEvent2 : UnityEvent2<AnimationEvent> { }
    }
}
