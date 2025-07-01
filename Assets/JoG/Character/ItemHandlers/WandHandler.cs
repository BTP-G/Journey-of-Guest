using JoG.Character.InputBanks;
using JoG.Magic;
using UnityEngine;

namespace JoG.Character.ItemHandlers {

    public class WandHandler : MonoBehaviour, IItemHandler {
        public Wand wand;
        [SerializeField] private Transform _wandFollow;
        private CharacterBody _body;
        private Vector3InputBank _aimInputBank;
        private TriggerInputBank _primaryActionInputBank;

        void IItemHandler.Handle(GameObject item) {
            if (item is null) {
                enabled = false;
                wand = null;
                return;
            }
            if (item.TryGetComponent(out Wand newWand)) {
                newWand.NetworkObject.ChangeOwnership(_body.OwnerClientId);
                wand = newWand;
                wand.Owner = _body.NetworkObject;
                enabled = true;
            } else {
                enabled = false;
            }
        }

        protected void Awake() {
            _body = GetComponentInParent<CharacterBody>();
            _aimInputBank = _body.GetInputBank<Vector3InputBank>("Aim");
            _primaryActionInputBank = _body.GetInputBank<TriggerInputBank>("PrimaryAction");
        }

        protected void OnEnable() {
            if (wand == null || !wand.NetworkObject.IsSpawned) {
                enabled = false;
                return;
            }
        }

        protected void Update() {
            wand.transform.position = _wandFollow.position;
            wand.transform.LookAt(_aimInputBank.vector3);
            if (_primaryActionInputBank.Triggered) {
                wand.Cast(_body);
            }
        }
    }
}