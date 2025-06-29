using UnityEngine;

namespace JoG {

    [RequireComponent(typeof(TrailRenderer))]
    public class ClearTrailRendererOnDisable : MonoBehaviour {
        private TrailRenderer _trailRenderer;

        private void Awake() {
            _trailRenderer = GetComponent<TrailRenderer>();
        }

        private void OnDisable() {
            _trailRenderer.Clear();
        }
    }
}