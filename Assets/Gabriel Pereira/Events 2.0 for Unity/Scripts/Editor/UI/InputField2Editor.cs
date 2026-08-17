using UnityEditor.AnimatedValues;
using UnityEngine.UI;

namespace UnityEditor.UI {
    [CanEditMultipleObjects]
    [CustomEditor(typeof(InputField2), true)]
    public class InputField2Editor : SelectableEditor {
        private SerializedProperty m_TextComponent;
        private SerializedProperty m_Text;
        private SerializedProperty m_ContentType;
        private SerializedProperty m_LineType;
        private SerializedProperty m_InputType;
        private SerializedProperty m_CharacterValidation;
        private SerializedProperty m_KeyboardType;
        private SerializedProperty m_CharacterLimit;
        private SerializedProperty m_CaretBlinkRate;
        private SerializedProperty m_CaretWidth;
        private SerializedProperty m_CaretColor;
        private SerializedProperty m_CustomCaretColor;
        private SerializedProperty m_SelectionColor;
        private SerializedProperty m_HideMobileInput;
        private SerializedProperty m_Placeholder;
        private SerializedProperty m_OnValueChanged;
        private SerializedProperty m_OnEndEdit;
        private SerializedProperty m_ReadOnly;

        private AnimBool m_CustomColor;

        protected override void OnEnable() {
            base.OnEnable();
            m_TextComponent = serializedObject.FindProperty("m_TextComponent");
            m_Text = serializedObject.FindProperty("m_Text");
            m_ContentType = serializedObject.FindProperty("m_ContentType");
            m_LineType = serializedObject.FindProperty("m_LineType");
            m_InputType = serializedObject.FindProperty("m_InputType");
            m_CharacterValidation = serializedObject.FindProperty("m_CharacterValidation");
            m_KeyboardType = serializedObject.FindProperty("m_KeyboardType");
            m_CharacterLimit = serializedObject.FindProperty("m_CharacterLimit");
            m_CaretBlinkRate = serializedObject.FindProperty("m_CaretBlinkRate");
            m_CaretWidth = serializedObject.FindProperty("m_CaretWidth");
            m_CaretColor = serializedObject.FindProperty("m_CaretColor");
            m_CustomCaretColor = serializedObject.FindProperty("m_CustomCaretColor");
            m_SelectionColor = serializedObject.FindProperty("m_SelectionColor");
            m_HideMobileInput = serializedObject.FindProperty("m_HideMobileInput");
            m_Placeholder = serializedObject.FindProperty("m_Placeholder");
            m_OnValueChanged = serializedObject.FindProperty("m_OnValueChanged");
            m_OnEndEdit = serializedObject.FindProperty("m_OnEndEdit");
            m_ReadOnly = serializedObject.FindProperty("m_ReadOnly");

            m_CustomColor = new AnimBool(m_CustomCaretColor.boolValue);
            m_CustomColor.valueChanged.AddListener(Repaint);
        }

        protected override void OnDisable() {
            base.OnDisable();

            m_CustomColor.valueChanged.RemoveListener(Repaint);
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            serializedObject.Update();

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(m_TextComponent);

            if (m_TextComponent != null && m_TextComponent.objectReferenceValue != null) {
                var text = m_TextComponent.objectReferenceValue as Text;
                if (text.supportRichText) {
                    EditorGUILayout.HelpBox("Using Rich Text with input is unsupported.", MessageType.Warning);
                }
            }

            using (new EditorGUI.DisabledScope(m_TextComponent == null || m_TextComponent.objectReferenceValue == null)) {
                EditorGUILayout.PropertyField(m_Text);
                EditorGUILayout.PropertyField(m_CharacterLimit);

                EditorGUILayout.Space();

                EditorGUILayout.PropertyField(m_ContentType);
                if (!m_ContentType.hasMultipleDifferentValues) {
                    EditorGUI.indentLevel++;

                    if (m_ContentType.enumValueIndex is ((int)InputField.ContentType.Standard) or
                        ((int)InputField.ContentType.Autocorrected) or
                        ((int)InputField.ContentType.Custom)) {
                        EditorGUILayout.PropertyField(m_LineType);
                    }

                    if (m_ContentType.enumValueIndex == (int)InputField.ContentType.Custom) {
                        EditorGUILayout.PropertyField(m_InputType);
                        EditorGUILayout.PropertyField(m_KeyboardType);
                        EditorGUILayout.PropertyField(m_CharacterValidation);
                    }

                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.Space();

                EditorGUILayout.PropertyField(m_Placeholder);
                EditorGUILayout.PropertyField(m_CaretBlinkRate);
                EditorGUILayout.PropertyField(m_CaretWidth);

                EditorGUILayout.PropertyField(m_CustomCaretColor);

                m_CustomColor.target = m_CustomCaretColor.boolValue;

                if (EditorGUILayout.BeginFadeGroup(m_CustomColor.faded)) {
                    EditorGUILayout.PropertyField(m_CaretColor);
                }
                EditorGUILayout.EndFadeGroup();

                EditorGUILayout.PropertyField(m_SelectionColor);
                EditorGUILayout.PropertyField(m_HideMobileInput);
                EditorGUILayout.PropertyField(m_ReadOnly);

                EditorGUILayout.Space();

                EditorGUILayout.PropertyField(m_OnValueChanged);
                EditorGUILayout.PropertyField(m_OnEndEdit);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
