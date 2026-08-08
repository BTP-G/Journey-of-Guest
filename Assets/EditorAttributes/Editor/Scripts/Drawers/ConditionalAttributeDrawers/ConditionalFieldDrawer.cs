using EditorAttributes.Editor.Utility;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;

namespace EditorAttributes.Editor {
    [CustomPropertyDrawer(typeof(ConditionalFieldAttribute))]
    public class ConditionalFieldDrawer : PropertyDrawerBase {
        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            var conditionalAttribute = attribute as ConditionalFieldAttribute;

            HelpBox errorBox = new();
            var propertyField = CreatePropertyField(property);

            UpdateVisualElement(propertyField, () => {
                var canActivateProperty = CanActivateProperty(conditionalAttribute, conditionalAttribute.BooleanNames, property, errorBox);

                switch (conditionalAttribute.ConditionResult) {
                    case ConditionResult.ShowHide:
                        propertyField.style.display = canActivateProperty ? DisplayStyle.Flex : DisplayStyle.None;
                        break;

                    case ConditionResult.EnableDisable:
                        propertyField.SetEnabled(canActivateProperty);
                        break;
                }

                DisplayErrorBox(propertyField, errorBox);
            });

            return propertyField;
        }

        private bool CanActivateProperty(ConditionalFieldAttribute attribute, string[] conditionNames, SerializedProperty property, HelpBox errorBox) {
            List<bool> booleanList = new();

            foreach (var conditionName in conditionNames) {
                var memberInfo = ReflectionUtils.GetValidMemberInfo(conditionName, property);
                var serializedProperty = property.serializedObject.FindProperty(conditionName);

                if (memberInfo == null) {
                    errorBox.text = $"The provided condition <b>{conditionName}</b> could not be found";
                    continue;
                }

                if (ReflectionUtils.GetMemberInfoType(memberInfo) == typeof(bool)) {
                    var propertyValue = (bool)ReflectionUtils.GetMemberInfoValue(memberInfo, property);

                    booleanList.Add(propertyValue);
                } else if (serializedProperty != null && serializedProperty.propertyType == SerializedPropertyType.Boolean) {
                    var propertyValue = serializedProperty.boolValue;

                    booleanList.Add(propertyValue);
                } else {
                    errorBox.text = $"The provided condition <b>{conditionName}</b> is not a valid boolean";
                }
            }

            for (var i = 0; i < booleanList.Count; i++) {
                if (!(attribute.NegatedValues == null || attribute.NegatedValues.Length == 0)) {
                    if (attribute.NegatedValues[i]) {
                        booleanList[i] = !booleanList[i];
                    }
                }

                switch (attribute.ConditionType) {
                    case ConditionType.AND: {
                        if (!booleanList[i]) {
                            return false;
                        }
                    }
                    continue;

                    case ConditionType.OR: {
                        if (booleanList[i]) {
                            return true;
                        }
                    }
                    continue;

                    case ConditionType.NAND: {
                        if (!booleanList[i]) {
                            return true;
                        }
                    }
                    continue;

                    case ConditionType.NOR: {
                        if (booleanList[i]) {
                            return false;
                        }
                    }
                    continue;
                }
            }

            return attribute.ConditionType switch {
                ConditionType.AND => true,
                ConditionType.NOR => true,
                _ => false,
            };
        }
    }
}
