using EditorAttributes;
using JoG.CameraSystem;
using UnityEngine;

namespace JoG.UI {

    [RequireComponent(typeof(Canvas))]
    public class WorldUICanvas : MonoBehaviour {
        [field: SerializeField, Required] public Canvas Canvas { get; private set; }

        //protected void Awake() {
        //    Canvas.worldCamera = Camera.main;
        //}

        protected void LateUpdate() {
            if (MainCameraManager.main.TryGet(out var cam)) {
                transform.rotation = cam.rotation;
            }
        }

        protected void OnValidate() {
            Canvas = GetComponent<Canvas>();
            Canvas.renderMode = RenderMode.WorldSpace;
        }
    }
}