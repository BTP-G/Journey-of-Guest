using EditorAttributes.Editor.Utility;
using UnityEditor;
using UnityEngine.UIElements;

namespace EditorAttributes.Editor {
    [CustomPropertyDrawer(typeof(MessageBoxAttribute))]
    public class MessageBoxDrawer : PropertyDrawerBase {
        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            var messageBoxAttribute = attribute as MessageBoxAttribute;

            var conditionalProperty = ReflectionUtils.GetValidMemberInfo(messageBoxAttribute.ConditionName, property);

            VisualElement root = new();
            HelpBox errorBox = new();
            HelpBox messageBox = new(string.Empty, (HelpBoxMessageType)messageBoxAttribute.MessageType);

            var propertyField = CreatePropertyField(property);

            if (CanApplyGlobalColor) {
                messageBox.style.color = EditorExtension.GLOBAL_COLOR;
                messageBox.style.backgroundColor = EditorExtension.GLOBAL_COLOR / 2f;
            }

            root.Add(propertyField);
            root.Add(messageBox);

            if (messageBoxAttribute.DrawAbove) {
                messageBox.PlaceBehind(propertyField);
            }

            UpdateVisualElement(propertyField, () => {
                if (GetConditionValue(conditionalProperty, messageBoxAttribute, property, errorBox)) {
                    messageBox.text = GetDynamicString(messageBoxAttribute.Message, property, messageBoxAttribute, errorBox);
                    messageBox.style.display = DisplayStyle.Flex;
                } else {
                    messageBox.style.display = DisplayStyle.None;
                }

                DisplayErrorBox(root, errorBox);
            });

            return root;
        }
    }
}
