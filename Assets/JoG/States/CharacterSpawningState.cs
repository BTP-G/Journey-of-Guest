using JoG.Character;
using UnityEngine;

namespace JoG.States {

    public class CharacterSpawningState : State {
        public float duration = 1;
        private float _spawnStartTime;
        private CharacterBody _body;

        protected override void Awake() {
            base.Awake();
            _body = GetComponentInParent<CharacterBody>();
        }

        protected override bool CheckTransitionIn() => _body.IsAlive;

        protected override bool CheckTransitionOut() => Time.time - _spawnStartTime > duration;

        protected void OnEnable() {
            _body.Animator.SetBool("isSpawning", true);
            _spawnStartTime = Time.time;
        }

        protected void OnDisable() {
            _body.Animator.SetBool("isSpawning", false);
        }
    }
}