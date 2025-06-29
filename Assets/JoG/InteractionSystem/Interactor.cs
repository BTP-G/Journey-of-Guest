using GuestUnion.ObjectPool.Generic;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Netcode;
using UnityEngine;

namespace JoG.InteractionSystem {

    public class Interactor : NetworkBehaviour {

        public void Interact(GameObject interactableObject) {
            using (ListPool<IInteractable>.Rent(out var interactables)) {
                interactableObject.GetComponents(interactables);
                if (interactables.Count is not 0) {
                    using (ListPool<IInteractionMessageHandler>.Rent(out var handlers)) {
                        gameObject.GetComponentsInChildren(handlers);
                        foreach (var interactable in interactables.AsSpan()) {
                            if (interactable.GetInteractability(this) is Interactability.Available) {
                                foreach (var handler in handlers.AsSpan()) {
                                    handler.Handle(interactable);
                                }
                                interactable.PreformInteraction(this);
                            }
                        }
                    }
                }
            }
        }
    }
}