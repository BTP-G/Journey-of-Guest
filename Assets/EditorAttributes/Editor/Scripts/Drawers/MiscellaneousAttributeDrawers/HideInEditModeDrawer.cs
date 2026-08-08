using UnityEditor;
using UnityEngine.UIElements;

namespace EditorAttributes.Editor {
    [CustomPropertyDrawer(typeof(HideInEditModeAttribute))]
    public class HideInEditModeDrawer : PropertyDrawerBase {
        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            var propertyField = CreatePropertyField(property);
            propertyField.style.display = EditorApplication.isPlayingOrWillChangePlaymode ? DisplayStyle.Flex : DisplayStyle.None;

            return propertyField;
        }
    }
}
