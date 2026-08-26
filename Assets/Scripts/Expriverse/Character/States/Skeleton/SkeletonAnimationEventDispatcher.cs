using System;
using UnityEngine;
using UnityEngine.Events;

namespace Expriverse.Character.States.Skeleton {

    public class SkeletonAnimationEventDispatcher : MonoBehaviour, IComponent {
        public AnimationEvent2 onSwordSwing = new();

        public void OnSwordSwing(AnimationEvent animationEvent) {
            onSwordSwing.Invoke(animationEvent);
        }

        [Serializable]
        public class AnimationEvent2 : UnityEvent2<AnimationEvent> { }
    }
}
