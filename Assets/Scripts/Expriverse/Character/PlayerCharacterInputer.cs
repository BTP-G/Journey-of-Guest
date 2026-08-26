using Expriverse.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using Xoderony.InputChannels;

namespace Expriverse.Character {

    [DefaultExecutionOrder(-10)]
    public class PlayerCharacterInputer : MonoBehaviour, IComponent, ICharacterInputDriver {
        [Inject] internal CinemachineOrbitalFollowAim aimer;
        [Inject, Key(Constants.InputAction.Move)] internal InputAction _move;
        [Inject, Key(Constants.InputAction.PrimaryAction)] internal InputAction _primaryAction;
        [Inject, Key(Constants.InputAction.SecondaryAction)] internal InputAction _secondaryAction;
        [Inject, Key(Constants.InputAction.Jump)] internal InputAction _jump;
        [Inject, Key(Constants.InputAction.Sprint)] internal InputAction _sprint;
        [Inject, Key(Constants.InputAction.Skill)] internal InputAction _skill;
        [Inject, Key(Constants.InputAction.Interact)] internal InputAction _interact;
        private InputChannel<bool> sprintInputChannel;
        private InputChannel<bool> jumpInputChannel;
        private InputChannel<bool> primarySkillInputChannel;
        private InputChannel<bool> secondarySkillInputChannel;
        private InputChannel<bool> specialSkillInputChannel;
        private InputChannel<Vector3> moveInputChannel;
        private InputChannel<AimInput> aimInputChannel;
        private InputChannel<bool> ineractInputChannel;

        void ICharacterInputDriver.Bind(CharacterEntity body) {
            var inputChannelHub = body.InputChannelHub;
            jumpInputChannel = inputChannelHub.GetInputChannel<bool>(InputKeys.Jump);
            moveInputChannel = inputChannelHub.GetInputChannel<Vector3>(InputKeys.Move);
            primarySkillInputChannel = inputChannelHub.GetInputChannel<bool>(InputKeys.PrimarySkill);
            secondarySkillInputChannel = inputChannelHub.GetInputChannel<bool>(InputKeys.SecondarySkill);
            specialSkillInputChannel = inputChannelHub.GetInputChannel<bool>(InputKeys.SpecialSkill);
            sprintInputChannel = inputChannelHub.GetInputChannel<bool>(InputKeys.Sprint);
            aimInputChannel = inputChannelHub.GetInputChannel<AimInput>(InputKeys.Aim);
            ineractInputChannel = inputChannelHub.GetInputChannel<bool>(InputKeys.Interact);
            enabled = true;
        }

        void ICharacterInputDriver.Unbind() {
            enabled = false;
        }

        private void Awake() {
            enabled = false;
        }

        private void OnEnable() {
            _primaryAction.performed += OnPrimaryAction;
            _primaryAction.canceled += OnPrimaryAction;
            _secondaryAction.performed += OnSecondaryAction;
            _secondaryAction.canceled += OnSecondaryAction;
            _jump.performed += OnJump;
            _jump.canceled += OnJump;
            _sprint.performed += OnSprint;
            _skill.performed += OnSkill;
            _skill.canceled += OnSkill;
            _interact.performed += OnInteract;
            _interact.canceled += OnInteract;
        }

        private void Update() {
            var moveInput = _move.ReadValue<Vector2>();
            var aimTarget = aimer.AimTarget?.transform;
            aimInputChannel.value = new AimInput(aimer.AimPoint, aimTarget);
            moveInputChannel.value = aimer.AimRotation * new Vector3(moveInput.x, 0, moveInput.y);
        }

        private void OnDisable() {
            _primaryAction.performed -= OnPrimaryAction;
            _primaryAction.canceled -= OnPrimaryAction;
            _secondaryAction.performed -= OnSecondaryAction;
            _secondaryAction.canceled -= OnSecondaryAction;
            _jump.performed -= OnJump;
            _jump.canceled -= OnJump;
            _sprint.performed -= OnSprint;
            _skill.performed -= OnSkill;
            _skill.canceled -= OnSkill;
            _interact.performed -= OnInteract;
            _interact.canceled -= OnInteract;
        }

        private void OnPrimaryAction(InputAction.CallbackContext context) {
            primarySkillInputChannel.value = context.performed;
        }

        private void OnSecondaryAction(InputAction.CallbackContext context) {
            secondarySkillInputChannel.value = context.performed;
        }

        private void OnJump(InputAction.CallbackContext context) {
            jumpInputChannel.value = context.performed;
        }

        private void OnSprint(InputAction.CallbackContext context) {
            sprintInputChannel.value = !sprintInputChannel.value;
        }

        private void OnInteract(InputAction.CallbackContext context) {
            ineractInputChannel.value = context.performed;
        }

        private void OnSkill(InputAction.CallbackContext context) {
            specialSkillInputChannel.value = context.performed;
        }
    }
}
