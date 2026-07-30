using UnityEngine;

namespace JoG.Effects {

    public class FogLayerSetup : MonoBehaviour {

        [Header("Fog Layer Settings")]
        [SerializeField] private int layerCount = 5;
        [SerializeField] private float layerSpacing = 0.8f;
        [SerializeField] private float baseHeight = 0f;
        [SerializeField] private float layerScale = 50f;

        [Header("Material Settings")]
        [SerializeField] private Color fogColor = new Color(0.5f, 0.6f, 0.7f, 0.3f);
        [SerializeField] private float fogDensity = 0.4f;
        [SerializeField] private float animationSpeed = 1f;
        [SerializeField] private float turbulence = 0.3f;

        [Header("Depth Settings")]
        [SerializeField] private float fogHeight = 3f;
        [SerializeField] private float fogOffset = 0f;
        [SerializeField] private float depthFogIntensity = 0.6f;
        [SerializeField] private float noiseScale = 1f;

        [Header("Flow Settings")]
        [SerializeField] private Vector2 flowDirection = new Vector2(0.1f, 0.05f);

        private DynamicFogController[] _fogLayers;

        [ContextMenu("Create Fog Layers")]
        public void CreateFogLayers() {
            ClearExistingLayers();

            _fogLayers = new DynamicFogController[layerCount];

            for (int i = 0; i < layerCount; i++) {
                float height = baseHeight + i * layerSpacing;
                CreateFogPlane(i, height);
            }

            SetupMultiLayerController();
        }

        private void CreateFogPlane(int index, float height) {
            GameObject fogPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            fogPlane.name = $"FogLayer_{index:D2}";
            fogPlane.transform.SetParent(transform);
            fogPlane.transform.position = new Vector3(0, height, 0);
            fogPlane.transform.rotation = Quaternion.Euler(0, 0, 0);
            fogPlane.transform.localScale = new Vector3(layerScale * 0.1f, 1, layerScale * 0.1f);

            Object.DestroyImmediate(fogPlane.GetComponent<Collider>());

            DynamicFogController fogController = fogPlane.AddComponent<DynamicFogController>();

            float heightFactor = 1f - (float)index / layerCount;
            fogController.SetFogColor(new Color(
                fogColor.r + Random.Range(-0.05f, 0.05f),
                fogColor.g + Random.Range(-0.05f, 0.05f),
                fogColor.b + Random.Range(-0.05f, 0.05f),
                fogDensity * (0.7f + heightFactor * 0.3f)
            ));
            fogController.SetFogDensity(fogDensity * (0.8f + heightFactor * 0.2f));
            fogController.SetAnimationSpeed(animationSpeed * (0.9f + Random.Range(-0.1f, 0.1f)));
            fogController.SetTurbulence(turbulence);
            fogController.SetDepthFog(fogHeight * (1f + heightFactor), fogOffset, depthFogIntensity * heightFactor);
            fogController.SetNoiseScale(noiseScale);
            fogController.SetFlowDirection(new Vector2(
                flowDirection.x + Random.Range(-0.02f, 0.02f),
                flowDirection.y + Random.Range(-0.02f, 0.02f)
            ));

            _fogLayers[index] = fogController;
        }

        private void SetupMultiLayerController() {
            MultiLayerFogController multiController = GetComponent<MultiLayerFogController>();
            if (multiController == null) {
                multiController = gameObject.AddComponent<MultiLayerFogController>();
            }
            multiController.fogLayers = _fogLayers;
        }

        [ContextMenu("Clear Fog Layers")]
        public void ClearExistingLayers() {
            DynamicFogController[] existing = GetComponentsInChildren<DynamicFogController>();
            foreach (var fog in existing) {
                if (fog.gameObject != gameObject) {
                    if (Application.isPlaying) {
                        Destroy(fog.gameObject);
                    } else {
                        DestroyImmediate(fog.gameObject);
                    }
                }
            }
            _fogLayers = null;
        }

        public void SetGlobalDensity(float density) {
            if (_fogLayers == null) return;
            foreach (var layer in _fogLayers) {
                if (layer != null) {
                    layer.SetFogDensity(density);
                }
            }
        }

        public void FadeGlobalDensity(float targetDensity, float duration) {
            if (_fogLayers == null) return;

            foreach (var layer in _fogLayers) {
                if (layer != null) {
                    layer.FadeFog(targetDensity, duration);
                }
            }
        }

        public void SetFogColor(Color color) {
            if (_fogLayers == null) return;
            foreach (var layer in _fogLayers) {
                if (layer != null) {
                    layer.SetFogColor(color);
                }
            }
        }

        public DynamicFogController[] GetFogLayers() {
            return _fogLayers;
        }

        private void OnDrawGizmosSelected() {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < layerCount; i++) {
                float height = baseHeight + i * layerSpacing;
                Gizmos.DrawWireCube(new Vector3(0, height, 0), new Vector3(layerScale, 0.1f, layerScale));
            }
        }
    }
}
