using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace EditorAttributes.Editor {
    [CustomPropertyDrawer(typeof(SceneDropdownAttribute))]
    public class SceneDropdownDrawer : CollectionDisplayDrawer {
        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            if (!IsSupportedPropertyType(property)) {
                return new HelpBox("The SceneDropdown Attribute can only be attached to a string or int", HelpBoxMessageType.Error);
            }

            HelpBox errorBox = new();
            var sceneNames = GetSceneList(errorBox);
            var dropdownField = CreateDropdownField(sceneNames, property);

            UpdateVisualElement(dropdownField, () => {
                var sceneNames = GetSceneList(errorBox);

                if (IsCollectionValid(sceneNames)) {
                    dropdownField.choices = sceneNames;
                }
            });

            DisplayErrorBox(dropdownField, errorBox);
            return dropdownField;
        }

        protected override void PasteValue(VisualElement element, SerializedProperty property, string clipboardValue) {
            var dropdown = element as DropdownField;

            var sceneName = int.TryParse(clipboardValue, out var sceneIndex) ? SceneNameFromIndex(sceneIndex) : clipboardValue;

            if (dropdown.choices.Contains(sceneName)) {
                dropdown.value = sceneName;
            } else {
                Debug.LogWarning($"Could not paste value <b>{clipboardValue}</b> since is not availiable as an option in the dropdown");
            }
        }

        protected override bool IsSupportedPropertyType(SerializedProperty property) {
            return property.propertyType is SerializedPropertyType.String or SerializedPropertyType.Integer;
        }

        protected override string SetDropdownDefaultValue(List<string> collectionValues, SerializedProperty property) {
            var propertyStringValue = property.propertyType == SerializedPropertyType.String ? property.stringValue : SceneNameFromIndex(property.intValue);
            return collectionValues.Contains(propertyStringValue) ? propertyStringValue : collectionValues[0];
        }

        protected override void SetPropertyValueFromDropdown(SerializedProperty property, DropdownField dropdown) {
            if (property.hasMultipleDifferentValues) {
                return;
            }

            if (property.propertyType == SerializedPropertyType.String) {
                property.stringValue = dropdown.value;
            } else {
                property.intValue = dropdown.index;
            }

            property.serializedObject.ApplyModifiedProperties();
        }

        protected override void SetDropdownValueFromProperty(SerializedProperty trackedProperty, DropdownField dropdownField) {
            var sceneName = trackedProperty.propertyType == SerializedPropertyType.Integer ? SceneNameFromIndex(trackedProperty.intValue) : trackedProperty.stringValue;

            if (dropdownField.choices.Contains(sceneName)) {
                dropdownField.SetValueWithoutNotify(sceneName);
            } else {
                Debug.LogWarning($"The value <b>{GetPropertyValueAsString(trackedProperty)}</b> set to the <b>{trackedProperty.name}</b> variable is not a valid scene identifier", trackedProperty.serializedObject.targetObject);
            }
        }

        private List<string> GetSceneList(HelpBox errorBox) {
            List<string> sceneList = new();
            var activeSceneList = EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes);

            if (activeSceneList == null || activeSceneList.Length == 0) {
                errorBox.text = "There are no scenes in the active build settings";
                return sceneList;
            }

            foreach (var scene in activeSceneList) {
                var sceneName = scene.Split('/')[^1].Split('.')[0]; // Remove the asset paths and file extension from the name

                sceneList.Add(sceneName);
            }

            return sceneList;
        }

        private string SceneNameFromIndex(int BuildIndex) {
            var path = SceneUtility.GetScenePathByBuildIndex(BuildIndex);

            if (path == string.Empty) {
                return "";
            }

            var slashIndex = path.LastIndexOf('/');
            var name = path[(slashIndex + 1)..];
            var dotIndex = name.LastIndexOf('.');

            return name[..dotIndex];
        }
    }
}
