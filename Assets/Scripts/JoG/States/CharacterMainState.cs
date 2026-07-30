using UnityEngine;
using UnityEngine.Events;
using VContainer;

namespace JoG.States {

    public class CharacterMainState : MonoBehaviour, IComponent {
        public UnityEvent2 onLifeStart = new();
        public UnityEvent2 onLifeEnd = new();
        [Inject] internal Animator animator;

        protected void OnEnable() {
            onLifeStart.Invoke();
            animator.SetBool(AnimatorHashs.isDead, false);
        }

        protected void OnDisable() {
            onLifeEnd.Invoke();
            animator.SetBool(AnimatorHashs.isDead, true);
        }

        protected void Reset() {
            gameObject.SetActive(false);
        }
    }
}
