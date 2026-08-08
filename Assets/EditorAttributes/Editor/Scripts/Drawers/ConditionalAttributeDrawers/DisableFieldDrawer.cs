using EditorAttributes.Editor.Utility;
using UnityEditor;
using UnityEngine.UIElements;

namespace EditorAttributes.Editor {
    [CustomPropertyDrawer(typeof(DisableFieldAttribute))]
    public class DisableFieldDrawer : PropertyDrawerBase {
        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            var disableAttribute = attribute as DisableFieldAttribute;
            var conditionalProperty = ReflectionUtils.GetValidMemberInfo(disableAttribute.ConditionName, property);

            HelpBox errorBox = new();
            var propertyField = CreatePropertyField(property);

            UpdateVisualElement(propertyField, () => {
                propertyField.SetEnabled(!GetConditionValue(conditionalProperty, disableAttribute, property, errorBox));
                DisplayErrorBox(propertyField, errorBox);
            });

            return propertyField;
        }
    }
}
