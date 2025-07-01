using EditorAttributes;
using JoG.Networking;
using TMPro;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.UI;

namespace JoG.UI {

    public class SessionCard : MonoBehaviour {
        public ISessionService sessionService;
        private string _sessionId;
        [field: SerializeField, Required] public TMP_Text NameText { get; private set; }
        [field: SerializeField, Required] public TMP_Text PlayerCountText { get; private set; }
        [field: SerializeField, Required] public Button JoinButton { get; private set; }

        public void UpdateView(ISessionInfo sessionInfo) {
            NameText.text = sessionInfo.Name;
            PlayerCountText.text = $"{sessionInfo.MaxPlayers - sessionInfo.AvailableSlots}/{sessionInfo.MaxPlayers}";
            _sessionId = sessionInfo.Id;
        }

        private void Awake() {
            JoinButton.onClick.AddListener(JoinSession);
        }

        private async void JoinSession() {
            var result = await LoadingPanelManager.Loading(sessionService.JoinSessionByIdAsync(_sessionId),"加入中······");
            if (result is "success") {
                var sessionCode = sessionService.SessionCode;
                PopupManager.PopupConfirm($"加入成功，是否将会话代码{sessionCode}复制到剪切板，以便他人加入使用。", () => GUIUtility.systemCopyBuffer = sessionCode);
            } else {
                PopupManager.PopupConfirm("加入失败：" + result);
            }
        }
    }
}