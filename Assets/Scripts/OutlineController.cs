using EditorAttributes;
using QuickOutline;
using UnityEngine;

namespace JoG {

    public class OutlineController : MonoBehaviour {
        [Required] public Outline outline;
        private float _outlineDuration;
        public Color OutlineColor { get => outline.OutlineColor; set => outline.OutlineColor = value; }
        public float OutlineWidth { get => outline.OutlineWidth; set => outline.OutlineWidth = value; }

        public void ShowOutline(float duration) {
            if (duration < _outlineDuration) return;
            _outlineDuration = duration;
            outline.enabled = true;
            enabled = true;
        }

        public void HideOutline() {
            _outlineDuration = 0;
            outline.enabled = false;
            enabled = false;
        }

        protected void Update() {
            if (_outlineDuration > 0) {
                _outlineDuration -= Time.deltaTime;
            } else {
                HideOutline();
            }
        }
    }
}