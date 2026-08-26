using System;
using UnityEngine;
using UnityEngine.Events;

namespace Expriverse.Character.States.Spitter {

    public class SpitterAnimationEventDispatcher : MonoBehaviour, IComponent {
        public AnimationEvent2 onShoot = new();

        public void Shoot(AnimationEvent animationEvent) {
            onShoot.Invoke(animationEvent);
        }
        private void PlayStep() {

        }
        private void Grunt() {

        }
        [Serializable]
        public class AnimationEvent2 : UnityEvent2<AnimationEvent> { }
    }
}
