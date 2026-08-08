#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace DamageNumbersPro {
    [CustomEditor(typeof(DamageNumberSprite), true), CanEditMultipleObjects]
    public class DamageNumberSpriteEditor : Editor {
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            var previousColor = GUI.color;
            var descriptionTextStyle = new GUIStyle(GUI.skin.label) {
                richText = true,
                wordWrap = true,
                stretchHeight = true,
                fixedHeight = 0
            };

            var targetSprite = (DamageNumberSprite)target;
            var damageNumber = targetSprite.GetComponentInParent<DamageNumber>();

            foreach (var target in targets) {
                var dnpSprite = (DamageNumberSprite)target;

                // Preview size
                if (dnpSprite.matchTextSize) {
                    var spriteR = dnpSprite.GetComponent<SpriteRenderer>();
                    if (spriteR != null) {
                        dnpSprite.UpdateSize(dnpSprite.GetComponentInParent<DamageNumber>(), spriteR);
                    }

                    var rectTransform = dnpSprite.GetComponent<RectTransform>();
                    if (rectTransform != null) {
                        dnpSprite.UpdateSize(dnpSprite.GetComponentInParent<DamageNumber>(), rectTransform);
                    }
                }
            }

            // Check material
            var spriteRenderer = targetSprite.GetComponent<SpriteRenderer>();
            if (damageNumber.enable3DGame && spriteRenderer != null && (spriteRenderer.sharedMaterial == null || spriteRenderer.sharedMaterial.shader.name.EndsWith("Overlay") == false)) {
                GUI.color = new Color(1, 0.7f, 0.7f, 1);
                EditorGUILayout.Space();
                EditorGUILayout.BeginVertical("Helpbox");
                EditorGUILayout.LabelField("Use the <b>Sprite Overlay</b> material if you want your sprite renderer to render in front of other objects.", descriptionTextStyle);

                if (GUILayout.Button("Use Sprite Overlay Material")) {
                    var guids = AssetDatabase.FindAssets($"t:Material DNP Sprite Overlay");

                    foreach (var guid in guids) {
                        var path = AssetDatabase.GUIDToAssetPath(guid);
                        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);

                        Undo.RecordObject(spriteRenderer, "Changed sprite renderer's material.");
                        spriteRenderer.sharedMaterial = mat;
                        break;
                    }
                }

                EditorGUILayout.EndVertical();
                GUI.color = previousColor;
            }

            GUI.color = new Color(1f, 1f, 1f, 0.7f);
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical("Helpbox");
            EditorGUILayout.LabelField("You can attach this component to <b>images</b> and <b>sprite renderes</b> inside your damage number prefab. It will handle <b>fading</b> them in and out with the damage number.", descriptionTextStyle);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("<b>Match Text Size</b> will resize the image or sprite renderer to match the text's size. For <b>images</b> it will always resize the <b>rect transform</b>. For <b>sprite renderers</b> it will either resize the <b>transform</b> or if possible the sprite's <b>width</b> and <b>height</b>.", descriptionTextStyle);
            EditorGUILayout.EndVertical();
            GUI.color = previousColor;

        }
    }
}
#endif