using EditorAttributes.Editor.Utility;
using UnityEditor;
using UnityEngine.UIElements;

namespace EditorAttributes.Editor {
    [CustomPropertyDrawer(typeof(HideFieldAttribute))]
    public class HideFieldDrawer : PropertyDrawerBase {
        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            var hideAttribute = attribute as HideFieldAttribute;
            var conditionalProperty = ReflectionUtils.GetValidMemberInfo(hideAttribute.ConditionName, property);

            HelpBox errorBox = new();
            var propertyField = CreatePropertyField(property);

            UpdateVisualElement(propertyField, () => {
                propertyField.style.display = !GetConditionValue(conditionalProperty, hideAttribute, property, errorBox) ? DisplayStyle.Flex : DisplayStyle.None;
                DisplayErrorBox(propertyField, errorBox);
            });

            return propertyField;
        }
    }
}
