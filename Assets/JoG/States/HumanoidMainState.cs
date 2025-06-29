using JoG.Character;
using JoG.Character.InputBanks;
using JoG.Character.Move;
using JoG.InteractionSystem;
using UnityEngine;

namespace JoG.States {

    public class HumanoidMainState : State {
        [SerializeField] private RigidbodyCharacterController characterController;
        [SerializeField] private CharacterInteractor interactor;
        [SerializeField] private RigidbodyJump jump;
        [SerializeField] private RigidbodySprint sprint;
        [SerializeField] private CharacterBody character;
        [SerializeField] private CharacterModel model;
        private Vector3InputBank moveInput;
        private Vector3InputBank aimInput;
        private TriggerInputBank jumpInput;
        private TriggerInputBank interactInput;
        private BooleanInputBank sprintInput;

        protected override void Awake() {
            base.Awake();
            moveInput = character.GetInputBank<Vector3InputBank>("Move");
            aimInput = character.GetInputBank<Vector3InputBank>("Aim");
            jumpInput = character.GetInputBank<TriggerInputBank>("Jump");
            interactInput = character.GetInputBank<TriggerInputBank>("Interact");
            sprintInput = character.GetInputBank<BooleanInputBank>("Sprint");
        }

        protected void OnEnable() {
            characterController.enabled = true;
        }

        protected override void Update() {
            var aimOrigin = character.AimOrigin;
            var aimTarget = aimInput.vector3;
            var aimDirection = (aimTarget - aimOrigin).normalized;
            characterController.UpdateAimDirection(aimDirection); ;
            characterController.UpdateMoveDirection(Quaternion.LookRotation(aimDirection, characterController.CharacterUp) * moveInput.vector3);
            character.AimTarget = aimTarget;
            sprint.IsSprinting = sprintInput.Value;
            if (jumpInput.Triggered && jump.TryJump()) {
                jumpInput.Reset();
            }
            if (interactInput.Triggered && interactor.FindInteractableObject(aimOrigin, aimDirection, out var result)) {
                interactor.Interact(result);
                interactInput.Reset();
            }
            UpdateAnimator();
            base.Update();
        }

        protected void OnDisable() {
            characterController.UpdateMoveDirection(Vector3.zero);
            characterController.enabled = false;
        }

        private void UpdateAnimator() {
            var animator = model.Animator;
            var localV = model.transform.InverseTransformDirection(characterController.currentVelocity) / characterController.maxStableMoveSpeed;
            animator.SetFloat(AnimationParameters.forwardSpeed, localV.z);
            animator.SetFloat(AnimationParameters.rightSpeed, localV.x);
            animator.SetFloat(AnimationParameters.upSpeed, localV.y);
            animator.SetFloat(AnimationParameters.maxMoveSpeed, characterController.maxStableMoveSpeed);
            animator.SetBool(AnimationParameters.isGrounded, characterController.IsStableOnGround);
            animator.SetBool(AnimationParameters.isSprinting, sprint.IsSprinting);
            //animator.SetBool(AnimationParameters.isCrouching, crouch.IsCrouching);
        }
    }
}