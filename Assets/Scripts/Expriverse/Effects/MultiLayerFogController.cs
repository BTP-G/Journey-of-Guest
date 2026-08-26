using UnityEngine;

namespace Expriverse.Effects {

    public class MultiLayerFogController : MonoBehaviour {

        [Header("Fog Layers")]
        [SerializeField] public DynamicFogController[] fogLayers;

        [Header("Global Settings")]
        [SerializeField] private Color baseFogColor = new Color(0.5f, 0.6f, 0.7f, 0.3f);
        [Range(0f, 1f)]
        [SerializeField] private float globalDensity = 0.5f;

        [Header("Animation")]
        [SerializeField] private bool animate = true;
        [Range(0f, 3f)]
        [SerializeField] private float globalSpeed = 1f;

        [Header("Variation per Layer")]
        [SerializeField] private float colorVariation = 0.1f;
        [SerializeField] private float densityVariation = 0.2f;
        [SerializeField] private float speedVariation = 0.3f;

        private void Start() {
            InitializeFogLayers();
        }

        private void InitializeFogLayers() {
            if (fogLayers == null || fogLayers.Length == 0) {
                Debug.LogWarning("No fog layers assigned to MultiLayerFogController");
                return;
            }

            for (var i = 0; i < fogLayers.Length; i++) {
                ConfigureFogLayer(fogLayers[i], i);
            }
        }

        private void ConfigureFogLayer(DynamicFogController layer, int index) {
            if (layer == null) {
                return;
            }

            var normalizedIndex = (float)index / Mathf.Max(1, fogLayers.Length - 1);

            var layerColor = baseFogColor;
            layerColor.r += Mathf.Sin(normalizedIndex * Mathf.PI) * colorVariation;
            layerColor.g += Mathf.Cos(normalizedIndex * Mathf.PI * 0.5f) * colorVariation;
            layer.SetFogColor(layerColor);

            var layerDensity = globalDensity * (1f + (Mathf.Sin(normalizedIndex * Mathf.PI * 2) * densityVariation));
            layer.SetFogDensity(layerDensity);

            var flowDir = new Vector2(
                Mathf.Cos(normalizedIndex * Mathf.PI * 0.7f),
                Mathf.Sin(normalizedIndex * Mathf.PI * 0.9f)
            );
            layer.SetFlowDirection(flowDir);

            var layerSpeed = globalSpeed * (1f + (Mathf.Sin(normalizedIndex * Mathf.PI * 1.3f) * speedVariation));
            layer.SetAnimationSpeed(layerSpeed);

            var height = 2f + (Mathf.Sin(normalizedIndex * Mathf.PI) * 1.5f);
            var offset = -1f + (normalizedIndex * 2f);
            layer.SetDepthFog(height, offset, 0.4f + (normalizedIndex * 0.3f));

            layer.SetNoiseScale(0.8f + (normalizedIndex * 0.4f));
            layer.SetTurbulence(0.2f + (normalizedIndex * 0.2f));
        }

        private void Update() {
            if (!animate) {
                return;
            }

            var time = Time.time * globalSpeed;

            for (var i = 0; i < fogLayers.Length; i++) {
                if (fogLayers[i] == null) {
                    continue;
                }

                var normalizedIndex = (float)i / Mathf.Max(1, fogLayers.Length - 1);
                var phase = normalizedIndex * Mathf.PI * 2;
                var timeOffset = Mathf.Sin(phase + (time * 0.5f)) * 0.1f;

                var dynamicFlow = new Vector2(
                    Mathf.Cos((time * 0.3f) + phase) * 0.1f,
                    Mathf.Sin((time * 0.2f) + (phase * 0.7f)) * 0.1f
                );
                fogLayers[i].SetFlowDirection(dynamicFlow);
            }
        }

        public void SetGlobalDensity(float density) {
            globalDensity = Mathf.Clamp01(density);
            InitializeFogLayers();
        }

        public void SetGlobalSpeed(float speed) {
            globalSpeed = Mathf.Max(0, speed);
        }

        public void SetFogColor(Color color) {
            baseFogColor = color;
            InitializeFogLayers();
        }

        public void SetDensity(float density) {
            globalDensity = Mathf.Clamp01(density);
            InitializeFogLayers();
        }

        public void FadeGlobalDensity(float targetDensity, float duration) {
            StartCoroutine(FadeDensityCoroutine(targetDensity, duration));
        }

        private System.Collections.IEnumerator FadeDensityCoroutine(float targetDensity, float duration) {
            var startDensity = globalDensity;
            float elapsed = 0;

            while (elapsed < duration) {
                elapsed += Time.deltaTime;
                var t = elapsed / duration;
                globalDensity = Mathf.Lerp(startDensity, targetDensity, t);
                InitializeFogLayers();
                yield return null;
            }

            globalDensity = targetDensity;
            InitializeFogLayers();
        }

        private void OnDrawGizmosSelected() {
            if (fogLayers == null) {
                return;
            }

            Gizmos.color = Color.cyan;
            for (var i = 0; i < fogLayers.Length; i++) {
                if (fogLayers[i] != null) {
                    Gizmos.DrawWireSphere(fogLayers[i].transform.position, 0.5f);
                }
            }
        }
    }
}
