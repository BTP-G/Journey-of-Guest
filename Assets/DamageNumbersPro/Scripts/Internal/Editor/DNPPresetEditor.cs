#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace DamageNumbersPro.Internal {
    [CustomEditor(typeof(DNPPreset))]
    public class DNPPresetEditor : Editor {
        public override void OnInspectorGUI() {
            // Prepare
            var labelStyle = new GUIStyle(GUI.skin.label) {
                richText = true
            };

            // Copying
            EditorGUILayout.Space(4);
            var dn = (DamageNumber)EditorGUILayout.ObjectField(null, typeof(DamageNumber), true, GUILayout.Height(80));
            var dropStyle = new GUIStyle(GUI.skin.box) {
                alignment = TextAnchor.MiddleCenter
            };
            var lastRect = GUILayoutUtility.GetLastRect();
            GUI.Box(lastRect, "Drop damage number here.", dropStyle);
            if (dn != null) {
                var preset = (DNPPreset)target;
                Undo.RegisterCompleteObjectUndo(preset, "Copied damage number.");
                preset.Get(dn);

                serializedObject.ApplyModifiedProperties();
            }

            // Get First Property
            var currentProperty = serializedObject.FindProperty("changeFontAsset");

            // Display Properties
            EditorGUILayout.BeginVertical();
            var visible = true;
            do {
                var isNewCategory = currentProperty.name.StartsWith("change") || currentProperty.name == "hideVerticalTexts";
                if (isNewCategory) {
                    visible = true;
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space();
                    EditorGUILayout.BeginVertical("Helpbox");
                }

                if (visible) {
                    if (isNewCategory) {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PrefixLabel("<size=14><b>" + currentProperty.displayName + "</b></size>", labelStyle);
                        EditorGUILayout.PropertyField(currentProperty, GUIContent.none, true);
                        EditorGUILayout.EndHorizontal();
                    } else {
                        EditorGUILayout.PropertyField(currentProperty, true);
                    }
                }

                if (isNewCategory) {
                    visible = currentProperty.boolValue;

                    if (visible && currentProperty.name.StartsWith("change")) {
                        DNPEditorInternal.Lines();
                    }
                }
            } while (currentProperty.NextVisible(false));

            EditorGUILayout.EndVertical();

            // Save Changes
            serializedObject.ApplyModifiedProperties();
        }
    }
}

#endif