using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EditorAttributes.Editor {
    [CustomPropertyDrawer(typeof(FolderPathAttribute))]
    public class FolderPathDrawer : PropertyDrawerBase {
        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            if (!IsSupportedPropertyType(property)) {
                return new HelpBox("The FolderPath Attribute can only be attached to a string", HelpBoxMessageType.Error);
            }

            var folderPathAttribute = attribute as FolderPathAttribute;

            VisualElement root = new();
            var propertyField = CreatePropertyField(property);

            Button button = new(SetFolderPath);
            Image buttonIcon = new() { image = EditorGUIUtility.IconContent("d_Folder Icon").image };

            button.style.width = 40f;
            button.style.height = 20f;
            propertyField.style.flexGrow = 1f;
            root.style.flexDirection = FlexDirection.Row;

            button.Add(buttonIcon);
            root.Add(propertyField);
            root.Add(button);

            return root;

            void SetFolderPath() {
                var folderPath = EditorUtility.OpenFolderPanel("Select folder", "Assets", "");

                if (folderPathAttribute.GetRelativePath && !string.IsNullOrEmpty(folderPath)) {
                    var projectRoot = Application.dataPath[..^"Assets".Length];

                    folderPath = Path.GetRelativePath(projectRoot, folderPath);
                }

                if (property.hasMultipleDifferentValues) {
                    return;
                }

                property.stringValue = folderPath;
                property.serializedObject.ApplyModifiedProperties();
            }
        }

        protected override bool IsSupportedPropertyType(SerializedProperty property) {
            return property.propertyType == SerializedPropertyType.String;
        }
    }
}
