using JoG.Character;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace JoG.InteractionSystem {

    [RequireComponent(typeof(CharacterBody))]
    public class CharacterInteractor : Interactor {
        public LayerMask interactiveLayer;
        [Range(0, float.MaxValue)] public float maxDistance = 3f;
        public CharacterBody Body { get; private set; }

        public void FindAndInteract(in Vector3 origin, in Vector3 direction) {
            if (FindInteractableObject(origin, direction, out var result)) {
                Interact(result);
            }
        }

        public bool FindInteractableObject(in Vector3 origin, in Vector3 direction, [NotNullWhen(true)] out GameObject result) {
            if (Physics.Raycast(origin, direction, out var hitInfo, maxDistance, interactiveLayer, QueryTriggerInteraction.Collide)) {
                result = hitInfo.collider.gameObject;
                return true;
            }
            result = null;
            return false;
        }

        public void FindAndInteract() => FindAndInteract(Body.AimOrigin, Body.AimVector);

        public bool FindInteractableObject(out GameObject result) =>
            FindInteractableObject(Body.AimOrigin, Body.AimVector, out result);

        protected void Awake() {
            Body = GetComponent<CharacterBody>();
        }

        protected void Reset() {
            interactiveLayer = LayerMask.GetMask("Default", "Trigger", "Item");
        }
    }
}