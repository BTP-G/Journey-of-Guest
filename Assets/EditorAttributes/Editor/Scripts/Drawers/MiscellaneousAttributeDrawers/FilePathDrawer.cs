using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EditorAttributes.Editor {
    [CustomPropertyDrawer(typeof(FilePathAttribute))]
    public class FilePathDrawer : PropertyDrawerBase {
        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            if (!IsSupportedPropertyType(property)) {
                return new HelpBox("The FilePath Attribute can only be attached to a string", HelpBoxMessageType.Error);
            }

            var filePathAttribute = attribute as FilePathAttribute;

            VisualElement root = new();
            var propertyField = CreatePropertyField(property);

            Button button = new(SetFilePath);
            Image buttonIcon = new() { image = EditorGUIUtility.IconContent("d_Folder Icon").image };

            button.style.width = 40f;
            button.style.height = 20f;
            propertyField.style.flexGrow = 1f;
            root.style.flexDirection = FlexDirection.Row;

            button.Add(buttonIcon);
            root.Add(propertyField);
            root.Add(button);

            return root;

            void SetFilePath() {
                var filePath = EditorUtility.OpenFilePanel("Select file", "Assets", filePathAttribute.Filters);

                if (filePathAttribute.GetRelativePath && !string.IsNullOrEmpty(filePath) && Path.IsPathFullyQualified(filePath)) {
                    var projectRoot = Application.dataPath[..^"Assets".Length];

                    filePath = Path.GetRelativePath(projectRoot, filePath);
                }

                if (property.hasMultipleDifferentValues) {
                    return;
                }

                property.stringValue = filePath;
                property.serializedObject.ApplyModifiedProperties();
            }
        }

        protected override bool IsSupportedPropertyType(SerializedProperty property) {
            return property.propertyType == SerializedPropertyType.String;
        }
    }
}
