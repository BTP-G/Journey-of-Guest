using JoG.States;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace JoG.Character {

    public class CharacterDeathState : State {
        public float despawnDelay = 5;
        private float _despawnTime;
        private CharacterBody _character;

        protected override bool CheckTransitionIn() => !_character.IsAlive;

        protected override bool CheckTransitionOut() => _character.IsAlive;

        protected override void Awake() {
            base.Awake();
            _character = GetComponentInParent<CharacterBody>();
        }

        protected void OnEnable() {
            _despawnTime = Time.time + despawnDelay;
            _character.Animator.SetBool("isDead", true);
        }

        protected override void Update() {
            base.Update();
            if (Time.time > _despawnTime) {
                _character.NetworkObject.Despawn();
            }
        }

        protected void OnDisable() {
            _character.Animator.SetBool("isDead", false);
        }
    }
}