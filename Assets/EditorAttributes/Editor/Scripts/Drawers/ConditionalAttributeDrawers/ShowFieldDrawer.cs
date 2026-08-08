using EditorAttributes.Editor.Utility;
using UnityEditor;
using UnityEngine.UIElements;

namespace EditorAttributes.Editor {
    [CustomPropertyDrawer(typeof(ShowFieldAttribute))]
    public class ShowFieldDrawer : PropertyDrawerBase {
        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            var showAttribute = attribute as ShowFieldAttribute;
            var conditionalProperty = ReflectionUtils.GetValidMemberInfo(showAttribute.ConditionName, property);

            HelpBox errorBox = new();
            var propertyField = CreatePropertyField(property);

            UpdateVisualElement(propertyField, () => {
                propertyField.style.display = GetConditionValue(conditionalProperty, showAttribute, property, errorBox) ? DisplayStyle.Flex : DisplayStyle.None;
                DisplayErrorBox(propertyField, errorBox);
            });

            return propertyField;
        }
    }
}
