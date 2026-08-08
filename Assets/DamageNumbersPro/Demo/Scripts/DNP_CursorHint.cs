using UnityEngine;

namespace DamageNumbersPro.Demo {
    public class DNP_CursorHint : MonoBehaviour {
        private CanvasGroup cg;

        private void Start() {
            cg = GetComponent<CanvasGroup>();
        }

        private void FixedUpdate() {
            if (Cursor.visible) {
                cg.alpha = Mathf.Max(cg.alpha - (Time.deltaTime * 2f), 0);
            } else {
                cg.alpha = Mathf.Min(cg.alpha + (Time.deltaTime * 2f), 1);
            }
        }
    }
}
