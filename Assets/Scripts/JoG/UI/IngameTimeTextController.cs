using EditorAttributes;
using System.Text;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using VContainer;

namespace JoG.UI {

    public class IngameTimeTextController : MonoBehaviour {

        [Required]
        public TMP_Text timeText;

        private NetworkTimeSystem timeSystem;
        private StringBuilder _sb = new();
        private int _currentSeconds;

        [Inject]
        internal void Inject(NetworkManager networkManager) {
            timeSystem = networkManager.NetworkTimeSystem;
        }

        private void Update() {
            var currentSeconds = (int)timeSystem.ServerTime;
            if (currentSeconds > _currentSeconds) {
                UpdateTimeText(currentSeconds);
                _currentSeconds = currentSeconds;
            }
        }

        private void UpdateTimeText(int totalSeconds) {
            var hours = totalSeconds / 3600;
            var minutes = (totalSeconds % 3600) / 60;
            var seconds = totalSeconds % 60;
            _sb.Clear()
                .Append(hours)
                .Append(':')
                .Append(minutes)
                .Append(':')
                .Append(seconds);
            timeText.SetText(_sb);
        }
    }
}
