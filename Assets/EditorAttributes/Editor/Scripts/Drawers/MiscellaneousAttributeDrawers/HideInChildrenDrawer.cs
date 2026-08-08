using EditorAttributes.Editor.Utility;
using UnityEditor;
using UnityEngine.UIElements;

namespace EditorAttributes.Editor {
    [CustomPropertyDrawer(typeof(HideInChildrenAttribute))]
    public class HideInChildrenDrawer : PropertyDrawerBase {
        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            var hideInChildrenAttribute = attribute as HideInChildrenAttribute;

            var propertyField = CreatePropertyField(property);
            propertyField.style.display = IsPropertyInherited(property, hideInChildrenAttribute) ? DisplayStyle.None : DisplayStyle.Flex;

            return propertyField;
        }

        private bool IsPropertyInherited(SerializedProperty property, HideInChildrenAttribute attribute) {
            var targetObjectType = property.serializedObject.targetObject.GetType();
            var fieldInfo = ReflectionUtils.FindField(property.name, property);

            foreach (var type in attribute.ChildTypes) {
                if (targetObjectType != type) {
                    return false;
                }
            }

            return targetObjectType != fieldInfo.DeclaringType;
        }
    }
}
