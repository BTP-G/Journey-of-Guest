using JoG.Character;
using JoG.InventorySystem;
using Unity.Netcode;
using UnityEngine;

namespace JoG.InteractionSystem {

    public class PickupItem : MonoBehaviour, IInteractable {
        public bool destroyAfterPickup;
        public ItemData itemData;
        public byte count = 1;
        private NetworkObject _networkObject;

        Interactability IInteractable.GetInteractability(Interactor interactor) {
            return interactor.TryGetComponent<IItemPickUpController>(out _)
                ? Interactability.Available
                : Interactability.ConditionsNotMet;
        }

        void IInteractable.PreformInteraction(Interactor interactor) {
            if (destroyAfterPickup) {
                _networkObject.Despawn();
            }
        }

        private void Awake() {
            _networkObject = GetComponent<NetworkObject>();
        }
    }
}