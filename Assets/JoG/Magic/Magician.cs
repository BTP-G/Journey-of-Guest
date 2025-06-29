using EditorAttributes;
using JoG.Character;
using JoG.InteractionSystem;
using System;
using UnityEngine;

namespace JoG.Magic {

    [RequireComponent(typeof(CharacterBody))]
    public class Magician : MonoBehaviour, IInteractionMessageHandler {
        public CharacterBody Body { get; private set; }
        [field: SerializeField, Required] public Transform WandFollow { get; private set; }
        [field: SerializeField] public Wand Wand { get; set; }

        void IInteractionMessageHandler.Handle(IInteractable interactableObject) {
            if (interactableObject is Wand) {
            }
        }

        protected void Awake() {
            Body = GetComponent<CharacterBody>();
        }
    }
}