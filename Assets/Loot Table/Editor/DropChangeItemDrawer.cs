using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TinyScript
{
    [CustomPropertyDrawer(typeof(DropChangeItem))]
    public class DropChangeItemDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var _oldColor = GUI.backgroundColor;
            EditorGUI.BeginProperty(position, label, property);
            //GUI.backgroundColor = Color.red;

            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            var weightRectLabel = new Rect(position.x, position.y, 55, 18);
            var weightRect = new Rect(position.x, position.y + 20, 55, 18);

            EditorGUI.LabelField(weightRectLabel, "Weight");
            if (property.FindPropertyRelative("Weight").floatValue < 0) { GUI.backgroundColor = Color.red; }
            EditorGUI.PropertyField(weightRect, property.FindPropertyRelative("Weight"), GUIContent.none);
            GUI.backgroundColor = _oldColor;

            var ItemRectLabel = new Rect(position.x + 60, position.y, position.width - 60, 18);
            var ItemRect = new Rect(position.x + 60, position.y + 20, position.width - 60 - 75, 18);

            EditorGUI.LabelField(ItemRectLabel, "Item To Drop");
            if (property.FindPropertyRelative("Drop").objectReferenceValue == null) { GUI.backgroundColor = Color.red; }
            EditorGUI.PropertyField(ItemRect, property.FindPropertyRelative("Drop"), GUIContent.none);
            GUI.backgroundColor = _oldColor;

            var MinMaxRectLabel = new Rect(position.x + position.width - 70, position.y, 70, 18);

            var MinRect = new Rect(position.x + position.width - 70, position.y + 20, 30, 18);
            var MinMaxRect = new Rect(position.x + position.width - 39, position.y + 20, 9, 18);
            var MaxRect = new Rect(position.x + position.width - 30, position.y + 20, 30, 18);

            if (property.FindPropertyRelative("MinCountItem").intValue < 0) { GUI.backgroundColor = Color.red; }
            if (property.FindPropertyRelative("MaxCountItem").intValue < property.FindPropertyRelative("MinCountItem").intValue) { GUI.backgroundColor = Color.red; }

            EditorGUI.LabelField(MinMaxRectLabel, "Min  -  Max");
            EditorGUI.PropertyField(MinRect, property.FindPropertyRelative("MinCountItem"), GUIContent.none);
            EditorGUI.LabelField(MinMaxRect, "-");
            EditorGUI.PropertyField(MaxRect, property.FindPropertyRelative("MaxCountItem"), GUIContent.none);
            GUI.backgroundColor = _oldColor;

            EditorGUI.EndProperty();
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 40;
        }
    }
}