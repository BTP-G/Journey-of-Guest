using UnityEditor;
using UnityEngine;

namespace BrunoMikoski.AnimationSequencer {
    // Modified by Pablo Huaxteco
    public sealed class AnimationSequencerPreferences : ScriptableObjectForPreferences<AnimationSequencerPreferences> {
        // Public static variables
        public static readonly string Version = "2.0.0";

        // Serialized fields
        [Header("While Editing")]
        [SerializeField]
        private bool onlyOneStepExpanded = true;
        [SerializeField]
        private bool onlyOneActionExpanded = true;
        [Header("When Previewing")]
        [SerializeField]
        private bool hideSteps;
        [SerializeField]
        private bool collapseSteps = true;
        [SerializeField]
        private bool visualizeStepsProgress = true;

        // Public properties
        public bool OnlyOneStepExpandedWhileEditing => onlyOneStepExpanded;
        public bool OnlyOneActionExpandedWhileEditing => onlyOneActionExpanded;
        public bool HideStepsWhenPreviewing => hideSteps;
        public bool CollapseStepsWhenPreviewing { get { return !hideSteps && collapseSteps; } }
        public bool VisualizeStepsProgressWhenPreviewing { get { return !hideSteps && visualizeStepsProgress; } }

        [SettingsProvider]
        private static SettingsProvider SettingsProvider() {
            return CreateSettingsProvider("Preferences/Animation Sequencer", OnGUI);
        }

        private static void OnGUI(SerializedObject serializedObject) {
            // Initial margin.
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUILayout.BeginVertical();

            // Modify label width.
            var originalLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 190;

            // Version label.
            EditorGUILayout.LabelField($"Version {Version}", EditorStyles.boldLabel);

            // Draw properties.
            var onlyOneStepExpandeProperty = serializedObject.FindProperty("onlyOneStepExpanded");
            EditorGUILayout.PropertyField(onlyOneStepExpandeProperty);
            var onlyOneActionExpandeProperty = serializedObject.FindProperty("onlyOneActionExpanded");
            EditorGUILayout.PropertyField(onlyOneActionExpandeProperty);
            var hideStepsProperty = serializedObject.FindProperty("hideSteps");
            EditorGUILayout.PropertyField(hideStepsProperty);
            if (!hideStepsProperty.boolValue) {
                var baseIndentLevel = EditorGUI.indentLevel;
                EditorGUI.indentLevel = baseIndentLevel + 1;
                var collapseStepsProperty = serializedObject.FindProperty("collapseSteps");
                var visualizeStepsProgressProperty = serializedObject.FindProperty("visualizeStepsProgress");
                EditorGUILayout.PropertyField(collapseStepsProperty);
                EditorGUILayout.PropertyField(visualizeStepsProgressProperty);
                EditorGUI.indentLevel = baseIndentLevel;
            }

            // Reset label width.
            EditorGUIUtility.labelWidth = originalLabelWidth;

            // End vertical block.
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }
    }
}
