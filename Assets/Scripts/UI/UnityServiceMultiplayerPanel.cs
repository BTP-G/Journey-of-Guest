using JoG.Networking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace JoG.UI {

    public class UnityServiceMultiplayerPanel : MonoBehaviour {
        [Inject] private SessionManager _sessionManager;
        [field: SerializeField] public TMP_InputField SessionNameInputField { get; private set; }
        [field: SerializeField] public Button CreateSessionButton { get; private set; }
        [field: SerializeField] public Button JoinSessionButton { get; private set; }

        private void Awake() {
            CreateSessionButton.onClick.AddListener(async () => {
                CreateSessionButton.interactable = false;
                JoinSessionButton.interactable = false;
                var result = await _sessionManager.CreateSessionAsync(SessionNameInputField.text);
                if (result is "success") return;
                else ConfirmPopupManager.Popup("创建失败：" + result);
                CreateSessionButton.interactable = true;
                JoinSessionButton.interactable = true;
            });
            JoinSessionButton.onClick.AddListener(async () => {
                CreateSessionButton.interactable = false;
                JoinSessionButton.interactable = false;
                var result = await _sessionManager.JoinSessionAsync(SessionNameInputField.text);
                if (result is "success") return;
                else ConfirmPopupManager.Popup("加入失败：" + result);
                CreateSessionButton.interactable = true;
                JoinSessionButton.interactable = true;
            });
        }
    }
}