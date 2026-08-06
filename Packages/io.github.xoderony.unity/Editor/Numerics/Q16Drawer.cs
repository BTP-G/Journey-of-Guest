using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Xoderony.Numerics;

namespace Xoderony.Numerics.Editor {

    [CustomPropertyDrawer(typeof(Q16))]
    public sealed class Q16Drawer : PropertyDrawer {

        private const float FieldSpacing = 4;

        private const float ValueFieldWidthPercent = 30;

        private const float PercentToRatio = 0.01f;

        private const string RawValuePropertyName = "_rawValue";

        private const string ValueLabelText = "Value";

        private const string RawValueTooltip = "Q16 fixed-point raw value. 65536 represents 1.";

        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            var rawValueProperty = property.FindPropertyRelative(RawValuePropertyName);

            var rawValueField = new PropertyField(rawValueProperty, property.displayName) {
                tooltip = RawValueTooltip,
            };
            rawValueField.style.flexGrow = 1;
            rawValueField.style.marginRight = FieldSpacing;

            var valueField = new FloatField(ValueLabelText) {
                tooltip = property.tooltip,
                showMixedValue = rawValueProperty.hasMultipleDifferentValues,
            };
            valueField.style.width = Length.Percent(ValueFieldWidthPercent);
            valueField.style.flexShrink = 1;

            var labelElement = valueField.labelElement;
            labelElement.style.width = StyleKeyword.Auto;
            labelElement.style.minWidth = 0;
            labelElement.style.flexGrow = 0;
            labelElement.style.flexShrink = 0;
            labelElement.style.marginRight = FieldSpacing;

            var initialValueScale = new Q16(rawValueProperty.intValue);
            var initialValue = initialValueScale.ToFloat();
            valueField.SetValueWithoutNotify(initialValue);

            valueField.RegisterValueChangedCallback(
                evt => {
                    var valueScale = new Q16(evt.newValue);
                    rawValueProperty.intValue = valueScale.RawValue;
                    rawValueProperty.serializedObject.ApplyModifiedProperties();
                    var value = valueScale.ToFloat();
                    valueField.SetValueWithoutNotify(value);
                }
            );
            valueField.TrackPropertyValue(
                rawValueProperty,
                changedProperty => {
                    valueField.showMixedValue = changedProperty.hasMultipleDifferentValues;
                    var valueScale = new Q16(changedProperty.intValue);
                    var value = valueScale.ToFloat();
                    valueField.SetValueWithoutNotify(value);
                }
            );

            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;
            container.Add(rawValueField);
            container.Add(valueField);
            return container;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            var rawValueProperty = property.FindPropertyRelative(RawValuePropertyName);
            var valueFieldWidth = position.width * ValueFieldWidthPercent * PercentToRatio;
            var rawValuePosition = new Rect(
                position.x,
                position.y,
                position.width - valueFieldWidth - FieldSpacing,
                position.height
            );
            var valuePosition = new Rect(
                rawValuePosition.xMax + FieldSpacing,
                position.y,
                valueFieldWidth,
                position.height
            );

            EditorGUI.BeginProperty(
                position,
                label,
                property
            );
            EditorGUI.showMixedValue = rawValueProperty.hasMultipleDifferentValues;

            var rawValueLabel = new GUIContent(
                label.text,
                label.image,
                RawValueTooltip
            );
            EditorGUI.PropertyField(
                rawValuePosition,
                rawValueProperty,
                rawValueLabel
            );

            var currentValueScale = new Q16(rawValueProperty.intValue);
            var value = currentValueScale.ToFloat();
            var valueLabel = new GUIContent(ValueLabelText, label.tooltip);
            var valueLabelSize = EditorStyles.label.CalcSize(valueLabel);
            EditorGUI.BeginChangeCheck();
            var labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = valueLabelSize.x + FieldSpacing;
            value = EditorGUI.FloatField(
                valuePosition,
                valueLabel,
                value
            );
            EditorGUIUtility.labelWidth = labelWidth;

            if (EditorGUI.EndChangeCheck()) {
                var valueScale = new Q16(value);
                rawValueProperty.intValue = valueScale.RawValue;
            }

            EditorGUI.showMixedValue = false;
            EditorGUI.EndProperty();
        }

    }

}
