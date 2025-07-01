using JoG.Character;
using JoG.Character.InputBanks;
using JoG.DebugExtensions;
using JoG.InteractionSystem;
using JoG.Magic;
using Unity.Netcode;
using UnityEngine;

namespace JoG.States {

    public class MaficianState : State, IItemHandler {
        public Wand wand;
        [SerializeField] private Transform _wandFollow;
        private CharacterBody _body;
        private Vector3InputBank _aimInputBank;
        private TriggerInputBank _primaryActionInputBank;

        void IItemHandler.Handle(GameObject item) {
            if (item is null) {
                wand = null;
                return;
            }
            item.TryGetComponent(out wand);
            this.Log($"Wand: {wand}");
        }

        protected override void Awake() {
            base.Awake();
            _body = GetComponentInParent<CharacterBody>();
            _aimInputBank = _body.GetInputBank<Vector3InputBank>("Aim");
            _primaryActionInputBank = _body.GetInputBank<TriggerInputBank>("PrimaryAction");
        }

        protected override bool CheckTransitionIn() => wand is not null;

        protected void OnEnable() {
            if (wand.IsSpawned) {
                wand.NetworkObject.ChangeOwnership(_body.OwnerClientId);
            } else {
                 
            }
            wand.Owner = _body.NetworkObject;
        }

        protected override void Update() {
            wand.transform.position = _wandFollow.position;
            wand.transform.LookAt(_aimInputBank.vector3);
            if (_primaryActionInputBank.Triggered) {
                wand.Cast(_body);
            }
            base.Update();
        }
    }
}