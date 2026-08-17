using UnityEditor.AnimatedValues;
using UnityEngine;
using UnityEngine.UI;

namespace UnityEditor.UI {
    [CustomEditor(typeof(ScrollRect2), true)]
    [CanEditMultipleObjects]
    public class ScrollRect2Editor : Editor {
        private SerializedProperty m_Content;
        private SerializedProperty m_Horizontal;
        private SerializedProperty m_Vertical;
        private SerializedProperty m_MovementType;
        private SerializedProperty m_Elasticity;
        private SerializedProperty m_Inertia;
        private SerializedProperty m_DecelerationRate;
        private SerializedProperty m_ScrollSensitivity;
        private SerializedProperty m_Viewport;
        private SerializedProperty m_HorizontalScrollbar;
        private SerializedProperty m_VerticalScrollbar;
        private SerializedProperty m_HorizontalScrollbarVisibility;
        private SerializedProperty m_VerticalScrollbarVisibility;
        private SerializedProperty m_HorizontalScrollbarSpacing;
        private SerializedProperty m_VerticalScrollbarSpacing;
        private SerializedProperty m_OnValueChanged;
        private AnimBool m_ShowElasticity;
        private AnimBool m_ShowDecelerationRate;
        private bool m_ViewportIsNotChild, m_HScrollbarIsNotChild, m_VScrollbarIsNotChild;
        private static string s_HError = "For this visibility mode, the Viewport property and the Horizontal Scrollbar property both needs to be set to a Rect Transform that is a child to the Scroll Rect.";
        private static string s_VError = "For this visibility mode, the Viewport property and the Vertical Scrollbar property both needs to be set to a Rect Transform that is a child to the Scroll Rect.";

        protected virtual void OnEnable() {
            m_Content = serializedObject.FindProperty("m_Content");
            m_Horizontal = serializedObject.FindProperty("m_Horizontal");
            m_Vertical = serializedObject.FindProperty("m_Vertical");
            m_MovementType = serializedObject.FindProperty("m_MovementType");
            m_Elasticity = serializedObject.FindProperty("m_Elasticity");
            m_Inertia = serializedObject.FindProperty("m_Inertia");
            m_DecelerationRate = serializedObject.FindProperty("m_DecelerationRate");
            m_ScrollSensitivity = serializedObject.FindProperty("m_ScrollSensitivity");
            m_Viewport = serializedObject.FindProperty("m_Viewport");
            m_HorizontalScrollbar = serializedObject.FindProperty("m_HorizontalScrollbar");
            m_VerticalScrollbar = serializedObject.FindProperty("m_VerticalScrollbar");
            m_HorizontalScrollbarVisibility = serializedObject.FindProperty("m_HorizontalScrollbarVisibility");
            m_VerticalScrollbarVisibility = serializedObject.FindProperty("m_VerticalScrollbarVisibility");
            m_HorizontalScrollbarSpacing = serializedObject.FindProperty("m_HorizontalScrollbarSpacing");
            m_VerticalScrollbarSpacing = serializedObject.FindProperty("m_VerticalScrollbarSpacing");
            m_OnValueChanged = serializedObject.FindProperty("m_OnValueChanged");

            m_ShowElasticity = new AnimBool(Repaint);
            m_ShowDecelerationRate = new AnimBool(Repaint);
            SetAnimBools(true);
        }

        protected virtual void OnDisable() {
            m_ShowElasticity.valueChanged.RemoveListener(Repaint);
            m_ShowDecelerationRate.valueChanged.RemoveListener(Repaint);
        }

        private void SetAnimBools(bool instant) {
            SetAnimBool(m_ShowElasticity, !m_MovementType.hasMultipleDifferentValues && m_MovementType.enumValueIndex == (int)ScrollRect.MovementType.Elastic, instant);
            SetAnimBool(m_ShowDecelerationRate, !m_Inertia.hasMultipleDifferentValues && m_Inertia.boolValue == true, instant);
        }

        private void SetAnimBool(AnimBool a, bool value, bool instant) {
            if (instant) {
                a.value = value;
            } else {
                a.target = value;
            }
        }

        private void CalculateCachedValues() {
            m_ViewportIsNotChild = false;
            m_HScrollbarIsNotChild = false;
            m_VScrollbarIsNotChild = false;
            if (targets.Length == 1) {
                var transform = ((ScrollRect2)target).transform;
                if (m_Viewport.objectReferenceValue == null || ((RectTransform)m_Viewport.objectReferenceValue).transform.parent != transform) {
                    m_ViewportIsNotChild = true;
                }

                if (m_HorizontalScrollbar.objectReferenceValue == null || ((Scrollbar2)m_HorizontalScrollbar.objectReferenceValue).transform.parent != transform) {
                    m_HScrollbarIsNotChild = true;
                }

                if (m_VerticalScrollbar.objectReferenceValue == null || ((Scrollbar2)m_VerticalScrollbar.objectReferenceValue).transform.parent != transform) {
                    m_VScrollbarIsNotChild = true;
                }
            }
        }

        public override void OnInspectorGUI() {
            SetAnimBools(false);

            serializedObject.Update();

            // Once we have a reliable way to know if the object changed, only re-cache in that case.
            CalculateCachedValues();

            EditorGUILayout.PropertyField(m_Content);

            EditorGUILayout.PropertyField(m_Horizontal);
            EditorGUILayout.PropertyField(m_Vertical);

            EditorGUILayout.PropertyField(m_MovementType);
            if (EditorGUILayout.BeginFadeGroup(m_ShowElasticity.faded)) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(m_Elasticity);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFadeGroup();

            EditorGUILayout.PropertyField(m_Inertia);
            if (EditorGUILayout.BeginFadeGroup(m_ShowDecelerationRate.faded)) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(m_DecelerationRate);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFadeGroup();

            EditorGUILayout.PropertyField(m_ScrollSensitivity);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(m_Viewport);

            EditorGUILayout.PropertyField(m_HorizontalScrollbar);
            if (m_HorizontalScrollbar.objectReferenceValue && !m_HorizontalScrollbar.hasMultipleDifferentValues) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(m_HorizontalScrollbarVisibility, new GUIContent("Visibility"));

                if ((ScrollRect.ScrollbarVisibility)m_HorizontalScrollbarVisibility.enumValueIndex == ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport
                    && !m_HorizontalScrollbarVisibility.hasMultipleDifferentValues) {
                    if (m_ViewportIsNotChild || m_HScrollbarIsNotChild) {
                        EditorGUILayout.HelpBox(s_HError, MessageType.Error);
                    }

                    EditorGUILayout.PropertyField(m_HorizontalScrollbarSpacing, new GUIContent("Spacing"));
                }

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(m_VerticalScrollbar);
            if (m_VerticalScrollbar.objectReferenceValue && !m_VerticalScrollbar.hasMultipleDifferentValues) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(m_VerticalScrollbarVisibility, new GUIContent("Visibility"));

                if ((ScrollRect.ScrollbarVisibility)m_VerticalScrollbarVisibility.enumValueIndex == ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport
                    && !m_VerticalScrollbarVisibility.hasMultipleDifferentValues) {
                    if (m_ViewportIsNotChild || m_VScrollbarIsNotChild) {
                        EditorGUILayout.HelpBox(s_VError, MessageType.Error);
                    }

                    EditorGUILayout.PropertyField(m_VerticalScrollbarSpacing, new GUIContent("Spacing"));
                }

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(m_OnValueChanged);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
