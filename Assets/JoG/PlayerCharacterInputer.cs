﻿using JoG.Character.InputBanks;
using JoG.Messages;
using MessagePipe;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

namespace JoG {

    public class PlayerCharacterInputer : MonoBehaviour, IMessageHandler<CharacterBodyChangedMessage> {
        private BooleanInputBank sprintInputBank;
        private TriggerInputBank interactInputBank;
        private TriggerInputBank jumpInputBank;
        private TriggerInputBank primaryActionInputBank;
        private TriggerInputBank secondaryActionInputBank;
        private TriggerInputBank skillInputBank;
        private Vector3InputBank moveInputBank;
        private InputActionMap _commonCharacterActionMap;
        private InputAction _move;
        private InputAction _primaryAction;
        private InputAction _secondaryAction;
        private InputAction _jump;
        private InputAction _sprint;
        private InputAction _skill;
        private InputAction _interact;
        private IDisposable _characterBodyChangedMessageDisposable;
        private int _enableInputCount;
        public static PlayerCharacterInputer Instance { get; private set; }

        void IMessageHandler<CharacterBodyChangedMessage>.Handle(CharacterBodyChangedMessage message) {
            var character = message.previous;
            if (character is not null && character.IsLocalPlayer) {
                interactInputBank = null;
                jumpInputBank = null;
                moveInputBank = null;
                primaryActionInputBank = null;
                secondaryActionInputBank = null;
                skillInputBank = null;
                sprintInputBank = null;
                UnregisterCallback();
                ReleaseEnableInput();
            }
            character = message.next;
            if (character is not null && character.IsLocalPlayer) {
                interactInputBank = character.GetInputBank<TriggerInputBank>("Interact");
                jumpInputBank = character.GetInputBank<TriggerInputBank>("Jump");
                moveInputBank = character.GetInputBank<Vector3InputBank>("Move");
                primaryActionInputBank = character.GetInputBank<TriggerInputBank>("PrimaryAction");
                secondaryActionInputBank = character.GetInputBank<TriggerInputBank>("SecondaryAction");
                skillInputBank = character.GetInputBank<TriggerInputBank>("Skill");
                sprintInputBank = character.GetInputBank<BooleanInputBank>("Sprint");
                RegisterCallback();
                RequestEnableInput();
            }
        }

        public void RequestEnableInput() {
            _enableInputCount++;
            UpdateInputState();
        }

        public void ReleaseEnableInput() {
            _enableInputCount--;
            UpdateInputState();
        }

        private void UpdateInputState() {
            if (_enableInputCount > 0) {
                _commonCharacterActionMap?.Enable();
            } else {
                _commonCharacterActionMap?.Disable();
            }
        }

        [Inject]
        private void Construct(InputActionAsset inputActionAsset, IBufferedSubscriber<CharacterBodyChangedMessage> subscriber) {
            _commonCharacterActionMap = inputActionAsset.FindActionMap("CommonCharacterActionMap", true);
            _move = _commonCharacterActionMap.FindAction("Move", true);
            _primaryAction = _commonCharacterActionMap.FindAction("PrimaryAction", true);
            _secondaryAction = _commonCharacterActionMap.FindAction("SecondaryAction", true);
            _jump = _commonCharacterActionMap.FindAction("Jump", true);
            _sprint = _commonCharacterActionMap.FindAction("Sprint", true);
            _skill = _commonCharacterActionMap.FindAction("Skill", true);
            _interact = _commonCharacterActionMap.FindAction("Interact", true);
            _characterBodyChangedMessageDisposable = subscriber.Subscribe(this);
        }

        private void Awake() {
            Instance = this;
        }

        private void OnDestroy() {
            _commonCharacterActionMap?.Disable();
            _characterBodyChangedMessageDisposable?.Dispose();
            Instance = null;
        }

        private void RegisterCallback() {
            _move.performed += OnMove;
            _move.canceled += OnMove;
            _primaryAction.performed += OnPrimaryAction;
            _primaryAction.canceled += OnPrimaryAction;
            _secondaryAction.performed += OnSecondaryAction;
            _secondaryAction.canceled += OnSecondaryAction;
            _jump.performed += OnJump;
            _jump.canceled += OnJump;
            _sprint.performed += OnSprint;
            _interact.performed += OnInteract;
            _interact.canceled += OnInteract;
            _skill.performed += OnSkill;
            _skill.canceled += OnSkill;
        }

        private void UnregisterCallback() {
            _move.performed -= OnMove;
            _move.canceled -= OnMove;
            _primaryAction.performed -= OnPrimaryAction;
            _primaryAction.canceled -= OnPrimaryAction;
            _secondaryAction.performed -= OnSecondaryAction;
            _secondaryAction.canceled -= OnSecondaryAction;
            _jump.performed -= OnJump;
            _jump.canceled -= OnJump;
            _sprint.performed -= OnSprint;
            _interact.performed -= OnInteract;
            _interact.canceled -= OnInteract;
            _skill.performed -= OnSkill;
            _skill.canceled -= OnSkill;
        }

        private void OnMove(InputAction.CallbackContext context) {
            var moveInput = context.ReadValue<Vector2>();
            moveInputBank.vector3 = new Vector3(moveInput.x, 0, moveInput.y);
        }

        private void OnPrimaryAction(InputAction.CallbackContext context) {
            primaryActionInputBank.UpdateState(context.performed);
        }

        private void OnSecondaryAction(InputAction.CallbackContext context) {
            secondaryActionInputBank.UpdateState(context.performed);
        }

        private void OnJump(InputAction.CallbackContext context) {
            jumpInputBank.UpdateState(context.performed);
        }

        private void OnSprint(InputAction.CallbackContext context) {
            sprintInputBank.UpdateState(!sprintInputBank.Value);
        }

        private void OnInteract(InputAction.CallbackContext context) {
            interactInputBank.UpdateState(context.performed);
        }

        private void OnSkill(InputAction.CallbackContext context) {
            skillInputBank.UpdateState(context.performed);
        }
    }
}