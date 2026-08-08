using UnityEditor;
using UnityEngine.UIElements;

namespace EditorAttributes.Editor {
    [CustomPropertyDrawer(typeof(IndentPropertyAttribute))]
    public class IndentPropertyDrawer : PropertyDrawerBase {
        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            var indentPropertyAttribute = attribute as IndentPropertyAttribute;

            var propertyField = CreatePropertyField(property);
            propertyField.style.marginLeft = indentPropertyAttribute.IndentLevel;

            return propertyField;
        }
    }
}
