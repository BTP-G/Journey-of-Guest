using UnityEngine.UI;

namespace UnityEditor.UI {
    [CustomEditor(typeof(Toggle2), true)]
    [CanEditMultipleObjects]
    public class Toggle2Editor : SelectableEditor {
        private SerializedProperty m_OnValueChangedProperty;
        private SerializedProperty m_TransitionProperty;
        private SerializedProperty m_GraphicProperty;
        private SerializedProperty m_GroupProperty;
        private SerializedProperty m_IsOnProperty;

        protected override void OnEnable() {
            base.OnEnable();

            m_TransitionProperty = serializedObject.FindProperty("toggleTransition");
            m_GraphicProperty = serializedObject.FindProperty("graphic");
            m_GroupProperty = serializedObject.FindProperty("m_Group");
            m_IsOnProperty = serializedObject.FindProperty("m_IsOn");
            m_OnValueChangedProperty = serializedObject.FindProperty("onValueChanged");
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            serializedObject.Update();

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(m_IsOnProperty);
            EditorGUILayout.PropertyField(m_TransitionProperty);
            EditorGUILayout.PropertyField(m_GraphicProperty);
            EditorGUILayout.PropertyField(m_GroupProperty);

            EditorGUILayout.Space();

            // Draw the event notification options
            EditorGUILayout.PropertyField(m_OnValueChangedProperty);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
