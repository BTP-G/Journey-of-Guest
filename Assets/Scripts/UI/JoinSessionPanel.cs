using JoG.Networking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace JoG.UI {

    public class JoinSessionPanel : MonoBehaviour {
        [Inject] private ISessionService _sessionService;
        [field: SerializeField] public TMP_InputField SessionCodeInputField { get; private set; }
        [field: SerializeField] public TMP_InputField PasswordInputField { get; private set; }
        [field: SerializeField] public Button JoinButton { get; private set; }
        [field: SerializeField] public Button ReturnButton { get; private set; }

        private void Awake() {
            JoinButton.onClick.AddListener(JoinSession);
            ReturnButton.onClick.AddListener(() => gameObject.SetActive(false));
            SessionCodeInputField.text = GUIUtility.systemCopyBuffer;
        }

        private async void JoinSession() {
            var result = await LoadingPanelManager.Loading(_sessionService.JoinSessionAsync(SessionCodeInputField.text, PasswordInputField.text),"加入中······");
            if (result is not "success") {
                ConfirmPopupManager.Popup("加入失败：" + result);
            };
        }
    }
}