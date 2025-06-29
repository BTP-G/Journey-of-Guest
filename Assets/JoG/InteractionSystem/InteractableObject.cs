using QuickOutline;
using Unity.Netcode;
using UnityEngine;

namespace JoG.InteractionSystem {

    [RequireComponent(typeof(Outline))]
    [RequireComponent(typeof(Collider))]
    public abstract class InteractableObject : NetworkBehaviour, IInteractable, IInfomationProvider {
        public Color outlineColor = Color.white;
        private Outline _outline;

        public abstract string GetString(string token);

        public abstract Interactability GetInteractability(Interactor interactor);

        public abstract void PreformInteraction(Interactor interactor);

        protected virtual void Awake() {
            _outline = GetComponent<Outline>();
        }

        protected virtual void OnEnable() {
            _outline.OutlineColor = outlineColor;
        }

        protected virtual void OnDisable() {
            _outline.OutlineColor = Color.gray;
        }
    }
}