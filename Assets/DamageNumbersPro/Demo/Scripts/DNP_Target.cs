using System.Collections;
using UnityEngine;

namespace DamageNumbersPro.Demo {
    public class DNP_Target : MonoBehaviour {
        public Vector3 movementOffset = new Vector3(0, 0, 0);

        private Material mat;
        private float defaultBrightness;

        private Coroutine hitRoutine;
        private Coroutine flipRoutine;
        private bool flipping;

        private Vector3 originalPosition;

        private void Start() {
            mat = GetComponent<MeshRenderer>().material;
            defaultBrightness = mat.GetFloat("_Brightness");

            flipping = false;

            originalPosition = transform.position;
        }

        private void Update() {
            // Move around
            transform.position = originalPosition + (movementOffset * Mathf.Sin(Time.time));
        }

        public void Hit() {
            if (hitRoutine != null) {
                StopCoroutine(hitRoutine);
            }

            hitRoutine = StartCoroutine(HitCoroutine());

            if (!flipping) {
                if (flipRoutine != null) {
                    StopCoroutine(flipRoutine);
                }

                flipRoutine = StartCoroutine(FlipCoroutine());
            }
        }

        private IEnumerator HitCoroutine() {
            var brightness = 1f;

            while (brightness < 3f) {
                // Glow up
                brightness = Mathf.Min(3, Mathf.Lerp(brightness, 3 + 0.1f, Time.deltaTime * 20f));
                mat.SetFloat("_Brightness", brightness);

                yield return null;
            }

            while (brightness > defaultBrightness) {
                // Glow down
                brightness = Mathf.Max(defaultBrightness, Mathf.Lerp(brightness, defaultBrightness - 0.1f, Time.deltaTime * 10f));
                mat.SetFloat("_Brightness", brightness);

                yield return null;
            }
        }

        private IEnumerator FlipCoroutine() {
            flipping = true;

            var angle = 0f;

            while (angle < 180f) {
                angle = Mathf.Min(180, Mathf.Lerp(angle, 190f, Time.deltaTime * 7f));
                transform.eulerAngles = new Vector3(angle, 0, 0);
                yield return null;

                if (angle > 150f) {
                    flipping = false;
                }
            }
        }
    }
}
