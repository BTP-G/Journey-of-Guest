using EditorAttributes.Editor.Utility;
using UnityEditor;
using UnityEngine.UIElements;

namespace EditorAttributes.Editor {
    [CustomPropertyDrawer(typeof(EnableFieldAttribute))]
    public class EnableFieldDrawer : PropertyDrawerBase {
        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            var enableAttribute = attribute as EnableFieldAttribute;
            var conditionalProperty = ReflectionUtils.GetValidMemberInfo(enableAttribute.ConditionName, property);

            HelpBox errorBox = new();
            var propertyField = CreatePropertyField(property);

            UpdateVisualElement(propertyField, () => {
                propertyField.SetEnabled(GetConditionValue(conditionalProperty, enableAttribute, property, errorBox));
                DisplayErrorBox(propertyField, errorBox);
            });

            return propertyField;
        }
    }
}
