using JoG.Character.InputBanks;
using JoG.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

namespace JoG.Character {

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
        private SprintInputBank sprintInputBank;
        private JumpInputBank jumpInputBank;
        private PrimarySkillInputBank primarySkillInputBank;
        private SecondarySkillInputBank secondarySkillInputBank;
        private SpecialSkillInputBank specialSkillInputBank;
        private MoveInputBank moveInputBank;
        private AimInputBank aimInputBank;
        private InteractInputBank ineractInputBank;

        void ICharacterInputDriver.Bind(CharacterEntity body) {
            var inputBankHub = body.InputBankHub;
            jumpInputBank = inputBankHub.GetInputBank<JumpInputBank>();
            moveInputBank = inputBankHub.GetInputBank<MoveInputBank>();
            primarySkillInputBank = inputBankHub.GetInputBank<PrimarySkillInputBank>();
            secondarySkillInputBank = inputBankHub.GetInputBank<SecondarySkillInputBank>();
            specialSkillInputBank = inputBankHub.GetInputBank<SpecialSkillInputBank>();
            sprintInputBank = inputBankHub.GetInputBank<SprintInputBank>();
            aimInputBank = inputBankHub.GetInputBank<AimInputBank>();
            ineractInputBank = inputBankHub.GetInputBank<InteractInputBank>();
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
            aimInputBank.vector3 = aimer.AimPoint;
            aimInputBank.target = aimer.AimTarget?.transform;
            moveInputBank.vector3 = aimer.AimRotation * new Vector3(moveInput.x, 0, moveInput.y);
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
            primarySkillInputBank.UpdateState(context.performed);
        }

        private void OnSecondaryAction(InputAction.CallbackContext context) {
            secondarySkillInputBank.UpdateState(context.performed);
        }

        private void OnJump(InputAction.CallbackContext context) {
            jumpInputBank.UpdateState(context.performed);
        }

        private void OnSprint(InputAction.CallbackContext context) {
            sprintInputBank.UpdateState(!sprintInputBank.Value);
        }

        private void OnInteract(InputAction.CallbackContext context) {
            ineractInputBank.UpdateState(context.performed);
        }

        private void OnSkill(InputAction.CallbackContext context) {
            specialSkillInputBank.UpdateState(context.performed);
        }
    }
}
