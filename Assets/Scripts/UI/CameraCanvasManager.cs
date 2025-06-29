using UnityEngine;

namespace JoG.UI {

    public class CameraCanvasManager : MonoBehaviour {
        public static Canvas CameraCanvas { get; private set; }

        private void Awake() {
            if (CameraCanvas) {
                Debug.LogError($"singleton class {nameof(CameraCanvasManager)}");
                return;
            }
            CameraCanvas = GetComponent<Canvas>();
            if (CameraCanvas.renderMode is not RenderMode.ScreenSpaceCamera) {
                Debug.LogError($"{nameof(CameraCanvas)}.renderMode is not RenderMode.ScreenSpaceCamera");
            }
        }

        private void OnDestroy() {
            CameraCanvas = null;
        }
    }
}