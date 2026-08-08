#if DOTWEEN_ENABLED
using UnityEditor;
using UnityEngine;

namespace BrunoMikoski.AnimationSequencer {
    // Created by Pablo Huaxteco
    [CustomPropertyDrawer(typeof(AnchorPosition))]
    public class AnchorPositionDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);

            var enumValueIndex = property.enumValueIndex;
            var lineHeight = EditorGUIUtility.singleLineHeight;

            // Calculate the size of the grid (limited to the available width).
            var gridSize = Mathf.Min(position.width - EditorGUIUtility.labelWidth, lineHeight * 3f);
            var buttonSize = gridSize / 3f;

            // Draw field name.
            var labeRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, lineHeight);
            EditorGUI.LabelField(labeRect, label);

            // Draw enum dropdown.
            var enumRect = new Rect(position.x + EditorGUIUtility.labelWidth, position.y + (lineHeight * 3),
                position.width - EditorGUIUtility.labelWidth, lineHeight);
            EditorGUI.PropertyField(enumRect, property, new GUIContent());

            // Draw grid.
            var gridRect = new Rect(position.x + EditorGUIUtility.labelWidth + ((enumRect.width - gridSize) * 0.5f), position.y, gridSize, gridSize);

            for (var i = 0; i < System.Enum.GetValues(typeof(AnchorPosition)).Length; i++) {
                var row = i / 3;
                var col = i % 3;

                var index = i;
                var isSelected = index == enumValueIndex;

                if (isSelected) {
                    GUI.backgroundColor = Color.green;
                }

                var buttonRect = new Rect(gridRect.x + (col * buttonSize), gridRect.y + (row * buttonSize), buttonSize - 2, buttonSize - 2);
                if (GUI.Button(buttonRect, GUIContent.none)) {
                    property.enumValueIndex = index;
                }

                GUI.backgroundColor = Color.white;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            // Enough height for the enum and the grid.
            return EditorGUIUtility.singleLineHeight * 4f;
        }
    }
}
#endif