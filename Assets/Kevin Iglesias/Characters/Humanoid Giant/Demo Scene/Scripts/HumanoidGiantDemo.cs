#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace KevinIglesias {
    [System.Serializable]
    public class MaterialType {
        [HideInInspector]
        public string materialName;

        public Material defaultMaterial;
        public Material uRPMaterial;

        public SkinnedMeshRenderer[] sMR;
        public MeshRenderer[] mR;
    }

    [ExecuteInEditMode]
    public class HumanoidGiantDemo : MonoBehaviour {
        [SerializeField]
        public List<MaterialType> materialTypes;

        private void OnValidate() {
            if (materialTypes == null) {
                return;
            }

            for (var i = 0; i < materialTypes.Count; i++) {

                if (materialTypes[i].defaultMaterial != null) {
                    materialTypes[i].materialName = materialTypes[i].defaultMaterial.name;
                }

                for (var j = 0; j < materialTypes[i].sMR.Length; j++) {
                    if (materialTypes[i].sMR[j] != null) {
                        if (GraphicsSettings.currentRenderPipeline == null) {
                            if (materialTypes[i].defaultMaterial != null) {
                                materialTypes[i].sMR[j].material = materialTypes[i].defaultMaterial;
                            }
                        } else {
                            if (materialTypes[i].uRPMaterial != null) {
                                materialTypes[i].sMR[j].material = materialTypes[i].uRPMaterial;
                            }
                        }
                    }
                }

                for (var j = 0; j < materialTypes[i].mR.Length; j++) {
                    if (materialTypes[i].mR[j] != null) {
                        if (GraphicsSettings.currentRenderPipeline == null) {
                            if (materialTypes[i].defaultMaterial != null) {
                                materialTypes[i].mR[j].material = materialTypes[i].defaultMaterial;
                            }
                        } else {
                            if (materialTypes[i].uRPMaterial != null) {
                                materialTypes[i].mR[j].material = materialTypes[i].uRPMaterial;
                            }
                        }
                    }
                }
            }
        }

        private void OnEnable() {
            OnValidate();
        }

        private void Update() { }
    }
}
#endif
