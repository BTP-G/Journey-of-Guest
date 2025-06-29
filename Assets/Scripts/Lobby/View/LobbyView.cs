using EditorAttributes;
using JoG.Lobby.Controller;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace JoG.Lobby.View {

    public class LobbyView : MonoBehaviour {
        private SteamLobbyController controller;
        [SerializeField, Required] private InputActionReference _displayActionReference;
        [SerializeField, Required] private GameObject _view;
        [SerializeField, Required] private LobbyMemberCard _cardPrefab;
        [SerializeField, Required] private Transform _inviteFriendCardTransform;

        private List<LobbyMemberCard> _memberCards = new();
        [field: SerializeField, Required] public RectTransform LobbyMemberCardContent { get; private set; }
        [field: SerializeField, Required] public TMP_Text LobbyIdText { get; private set; }
        [field: SerializeField, Required] public TMP_InputField LobbyNameInputField { get; private set; }
        [field: SerializeField, Required] public TMP_InputField LobbyMemberLimitInputField { get; private set; }
        [field: SerializeField, Required] public TMP_Dropdown LobbyTypeDropdown { get; private set; }
        [field: SerializeField, Required] public Button InviteFriendsButton { get; private set; }

        public void Refresh() {
            if (controller is null || !controller.Lobby.Id.IsValid) {
                Init("0", "", 0, ELobbyType.Private, false);
                foreach (var card in _memberCards) {
                    card.gameObject.SetActive(false);
                }
                InviteFriendsButton.interactable = false;
                return;
            }
            Init(controller.LobbyId
               , controller.LobbyName
               , controller.MaxMembers
               , controller.LobbyType
               , controller.IsServer);
            while (_memberCards.Count < controller.MemberCount) {
                _memberCards.Add(Instantiate(_cardPrefab, LobbyMemberCardContent));
            }
            var i = 0;
            foreach (var member in controller.Members) {
                _memberCards[i++].UpdateCard(member);
            }
            while (i < _memberCards.Count) {
                _memberCards[i++].gameObject.SetActive(false);
            }
            InviteFriendsButton.interactable = true;
            _inviteFriendCardTransform.SetAsLastSibling();
        }

        private void Awake() {
            _displayActionReference.action.performed += OnDisplayStateChanged;
        }

        private void OnDestroy() {
            _displayActionReference.action.performed -= OnDisplayStateChanged;
        }

        private void Init(string id, string name, byte maxMemers, ELobbyType lobbyType, bool interactable) {
            LobbyIdText.text = id;
            LobbyNameInputField.text = name;
            LobbyMemberLimitInputField.text = maxMemers.ToString();
            LobbyTypeDropdown.value = (byte)lobbyType;
            LobbyNameInputField.readOnly = !interactable;
            LobbyMemberLimitInputField.readOnly = !interactable;
            LobbyTypeDropdown.interactable = interactable;
        }

        private void OnLobbyControllerChanged(Component component) {
            controller = component as SteamLobbyController;
            LobbyNameInputField.onEndEdit.AddListener(controller.SetLobbyName);
            LobbyMemberLimitInputField.onEndEdit.AddListener(controller.SetLobbyMaxMembersFromString);
            LobbyTypeDropdown.onValueChanged.AddListener(controller.SetLobbyTypeFromInt);
            InviteFriendsButton.onClick.AddListener(controller.OpenInviteFriendsUI);
        }

        private void OnDisplayStateChanged(InputAction.CallbackContext context) {
            if (_view.activeSelf) {
                _view.SetActive(false);
            } else {
                _view.SetActive(true);
                Refresh();
            }
        }
    }
}