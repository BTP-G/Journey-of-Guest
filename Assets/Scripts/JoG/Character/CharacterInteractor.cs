using JoG.Character.InputBanks;
using JoG.Interaction;
using JoG.UI;
using UnityEngine;
using VContainer;
using Xoderony;
using Xoderony.Extensions;
using Xoderony.ObjectPool.Generic;

namespace JoG.Character {

    public class CharacterInteractor : MonoBehaviour, IComponent {
        public float maxSqrtDistance = 16;
        [Inject] internal Entity entity;
        [Inject] internal CharacterModel model;
        [Inject] internal WorldTooltip _tooltip;
        private Transform _currentTarget;
        private InteractInputBank _interactInput;
        private AimInputBank _aimInput;
        private IDelegateDispatcher<InteractionHandler> _interactionHandlers;

        [Inject]
        internal void Inject(
            InputBankHub inputBankHub,
            IDelegateDispatcher<InteractionHandler> interactionHandlers) {

            _interactInput = inputBankHub.GetInputBank<InteractInputBank>();
            _aimInput = inputBankHub.GetInputBank<AimInputBank>();
            _interactionHandlers = interactionHandlers;
        }

        private void Update() {
            var interactTarget = _aimInput.target;
            var hasTarget = interactTarget != null && model.Center.SqrDistanceTo(interactTarget.transform.position) < maxSqrtDistance;
            if (hasTarget) {
                if (_currentTarget != null) {
                    OnExitInteraction(_currentTarget);
                }
                _currentTarget = interactTarget;
                OnEnterInteraction(_currentTarget);
            } else if (_currentTarget != null) {
                OnExitInteraction(_currentTarget);
                _currentTarget = null;
            }
            if (_interactInput.Value && hasTarget) {
                Interact(interactTarget);
            }
        }

        private void Interact(Transform interactableObject) {
            using (ListPool<IInteractable>.Rent(out var interactables)) {
                interactableObject.GetComponents(interactables);
                foreach (var interactable in interactables) {
                    if (interactable.CanInteract(entity)) {
                        _interactionHandlers.Handlers?.Invoke(interactable);
                        interactable.OnInteracted(entity);
                        _interactInput.UpdateState(false);
                    }
                }
            }
        }

        private void OnEnterInteraction(Transform collider) {
            if (collider.TryGetComponent<IWorldTooltipSource>(out var tooltipSource)) {
                _tooltip.SetTooltip(tooltipSource);
                _tooltip.Show(0.1f);
            }
            if (collider.TryGetComponent<Outline>(out var outline)) {
                outline.enabled = true;
            }
        }

        private void OnExitInteraction(Transform collider) {
            _tooltip.Hide(0.1f);
            if (collider.TryGetComponent<Outline>(out var outline)) {
                outline.enabled = false;
            }
        }
    }
}
