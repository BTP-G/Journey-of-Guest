using ANU.IngameDebug.Console;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Expriverse {

    [DebugCommandPrefix("debug")]
    public class TickSetter : MonoBehaviour {
        public int InterpolationBufferTickOffset;
        public bool PrintDeltaTime;

        private void Awake() {
            Application.targetFrameRate = 60;
            if (Application.isPlaying) {
                NetworkTransform.InterpolationBufferTickOffset = InterpolationBufferTickOffset;
            }
        }

        private void OnValidate() {
            if (Application.isPlaying) {
                NetworkTransform.InterpolationBufferTickOffset = InterpolationBufferTickOffset;
            }
        }

        [DebugCommand]
        private void LoadScene(int index) {
            SceneManager.LoadScene(index);
        }
    }
}
