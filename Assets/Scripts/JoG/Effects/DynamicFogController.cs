using UnityEngine;

namespace JoG.Effects {

    public class DynamicFogController : MonoBehaviour {

        [Header("Fog Settings")]
        [SerializeField] private Color fogColor = new Color(0.5f, 0.6f, 0.7f, 0.5f);
        [Range(0f, 1f)]
        [SerializeField] private float fogDensity = 0.3f;

        [Header("Flow Direction")]
        [SerializeField] private Vector2 flowDirection = new Vector2(0.1f, 0.05f);
        [Range(0f, 5f)]
        [SerializeField] private float animationSpeed = 1f;
        [Range(0f, 1f)]
        [SerializeField] private float turbulence = 0.3f;

        [Header("Depth Fog")]
        [Range(0f, 10f)]
        [SerializeField] private float fogHeight = 2f;
        [Range(-5f, 5f)]
        [SerializeField] private float fogOffset = 0f;
        [Range(0f, 1f)]
        [SerializeField] private float depthFogIntensity = 0.5f;

        [Header("Noise Scale")]
        [Range(0.01f, 10f)]
        [SerializeField] private float noiseScale = 1f;

        private Renderer _fogRenderer;
        private Material _fogMaterial;

        private int _fogColorID;
        private int _fogDensityID;
        private int _fogSpeedID;
        private int _fogNoiseScaleID;
        private int _depthFogHeightID;
        private int _depthFogOffsetID;
        private int _depthFogIntensityID;
        private int _animationSpeedID;
        private int _turbulenceID;

        private void Awake() {
            CacheShaderProperties();
            SetupFog();
        }

        private void CacheShaderProperties() {
            _fogColorID = Shader.PropertyToID("_FogColor");
            _fogDensityID = Shader.PropertyToID("_FogDensity");
            _fogSpeedID = Shader.PropertyToID("_FogSpeed");
            _fogNoiseScaleID = Shader.PropertyToID("_FogNoiseScale");
            _depthFogHeightID = Shader.PropertyToID("_DepthFogHeight");
            _depthFogOffsetID = Shader.PropertyToID("_DepthFogOffset");
            _depthFogIntensityID = Shader.PropertyToID("_DepthFogIntensity");
            _animationSpeedID = Shader.PropertyToID("_AnimationSpeed");
            _turbulenceID = Shader.PropertyToID("_Turbulence");
        }

        private void SetupFog() {
            _fogRenderer = GetComponent<Renderer>();
            if (_fogRenderer == null) {
                Debug.LogError("DynamicFogController requires a Renderer on the same GameObject!");
                return;
            }

            var shader = Shader.Find("JoG/Environment/DynamicFog");
            if (shader == null) {
                Debug.LogError("Shader 'JoG/Environment/DynamicFog' not found!");
                return;
            }

            _fogMaterial = new Material(shader);
            _fogRenderer.material = _fogMaterial;

            UpdateMaterialProperties();
        }

        private void UpdateMaterialProperties() {
            if (_fogMaterial == null) {
                return;
            }

            _fogMaterial.SetColor(_fogColorID, fogColor);
            _fogMaterial.SetFloat(_fogDensityID, fogDensity);
            _fogMaterial.SetVector(_fogSpeedID, new Vector4(flowDirection.x, flowDirection.y, 0, 0));
            _fogMaterial.SetFloat(_fogNoiseScaleID, noiseScale);
            _fogMaterial.SetFloat(_depthFogHeightID, fogHeight);
            _fogMaterial.SetFloat(_depthFogOffsetID, fogOffset);
            _fogMaterial.SetFloat(_depthFogIntensityID, depthFogIntensity);
            _fogMaterial.SetFloat(_animationSpeedID, animationSpeed);
            _fogMaterial.SetFloat(_turbulenceID, turbulence);
        }

        private void Update() {
            UpdateMaterialProperties();
        }

        public void SetFogColor(Color color) {
            fogColor = color;
        }

        public void SetFogDensity(float density) {
            fogDensity = Mathf.Clamp01(density);
        }

        public void SetFlowDirection(Vector2 direction) {
            flowDirection = direction;
        }

        public void SetAnimationSpeed(float speed) {
            animationSpeed = Mathf.Max(0, speed);
        }

        public void SetDepthFog(float height, float offset, float intensity) {
            fogHeight = height;
            fogOffset = offset;
            depthFogIntensity = intensity;
        }

        public void SetNoiseScale(float scale) {
            noiseScale = Mathf.Max(0.01f, scale);
        }

        public void SetTurbulence(float turb) {
            turbulence = Mathf.Clamp01(turb);
        }

        public void FadeFog(float targetDensity, float duration) {
            StartCoroutine(FadeFogCoroutine(targetDensity, duration));
        }

        private System.Collections.IEnumerator FadeFogCoroutine(float targetDensity, float duration) {
            var startDensity = fogDensity;
            float elapsed = 0;

            while (elapsed < duration) {
                elapsed += Time.deltaTime;
                var t = elapsed / duration;
                fogDensity = Mathf.Lerp(startDensity, targetDensity, t);
                yield return null;
            }

            fogDensity = targetDensity;
        }

        private void OnDestroy() {
            if (_fogMaterial != null) {
                Destroy(_fogMaterial);
            }
        }

#if UNITY_EDITOR
        private void OnValidate() {
            if (Application.isPlaying && _fogMaterial != null) {
                UpdateMaterialProperties();
            }
        }
#endif
    }
}
