using UnityEditor;
using UnityEngine.UIElements;

namespace EditorAttributes.Editor {
    [CustomPropertyDrawer(typeof(HideInPlayModeAttribute))]
    public class HideInPlayModeDrawer : PropertyDrawerBase {
        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            var propertyField = CreatePropertyField(property);
            propertyField.style.display = !EditorApplication.isPlayingOrWillChangePlaymode ? DisplayStyle.Flex : DisplayStyle.None;

            return propertyField;
        }
    }
}
