using EditorAttributes;
using UnityEngine;
using VContainer;

namespace Expriverse.States {

    public class CharacterSpawnState : State {
        [Required] public State next;
        [Inject] internal Animator _animator;

        protected void Update() {
            var state = _animator.GetCurrentAnimatorStateInfo(0);
            if (state.tagHash == AnimatorHashs.SpawnState) {
                return;
            }
            //TransitionTo(next);
        }
    }
}
