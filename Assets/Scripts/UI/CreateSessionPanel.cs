using JoG.Networking;
using TMPro;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace JoG.UI {

    public class CreateSessionPanel : MonoBehaviour {
        [Inject] private ISessionService _sessionManager;
        [Inject] private IAuthenticationService _authenticationService;
        [field: SerializeField] public TMP_InputField SessionNameInputField { get; private set; }
        [field: SerializeField] public TMP_InputField PasswordInputField { get; private set; }
        [field: SerializeField] public TMP_InputField MaxPlayersInputField { get; private set; }
        [field: SerializeField] public Toggle IsPrivateToggle { get; private set; }
        [field: SerializeField] public Button CreateButton { get; private set; }
        [field: SerializeField] public Button ReturnButton { get; private set; }

        private void Awake() {
            CreateButton.onClick.AddListener(CreateSession);
            ReturnButton.onClick.AddListener(() => gameObject.SetActive(false));
            SessionNameInputField.text = PlayerPrefs.GetString("session_name", _authenticationService.PlayerName + "'s session");
            MaxPlayersInputField.text = PlayerPrefs.GetString("session_max_players", "4");
            SessionNameInputField.onEndEdit.AddListener((text) => PlayerPrefs.SetString("session_name", text));
            MaxPlayersInputField.onEndEdit.AddListener((text) => PlayerPrefs.SetString("session_max_players", text));
        }

        private async void CreateSession() {
            var result = await LoadingPanelManager.Loading(
                _sessionManager.CreateSessionAsync(
                    SessionNameInputField.text,
                    PasswordInputField.text,
                    byte.Parse(MaxPlayersInputField.text),
                    IsPrivateToggle.isOn
                    )
                ,"创建中······");
            if (result is "success") {
                var sessionCode = _sessionManager.SessionCode;
                PopupManager.PopupConfirm($"创建成功，是否将会话代码{sessionCode}复制到剪切板，以便他人加入使用。", () => GUIUtility.systemCopyBuffer = sessionCode);
            } else {
                PopupManager.PopupConfirm($"创建失败：{result}。是否重试？", CreateSession);
            }
        }
    }
}