using JoG.States;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using VContainer;

namespace JoG.Character {

    public class CharacterDeathState : State {
        public UnityEvent2 onDeathEnter = new();
        public UnityEvent2 onDeathExit = new();
        [Inject] internal NetworkObject networkObject;
        [Inject] internal Animator animator;

        protected void OnEnable() {
            animator.SetBool(AnimatorHashs.isDead, true);
            onDeathEnter.Invoke();
            //TransitionTo(null);
        }

        protected void OnDisable() {
            animator.SetBool(AnimatorHashs.isDead, false);
            onDeathExit.Invoke();
            networkObject.Despawn();
        }
    }
}
