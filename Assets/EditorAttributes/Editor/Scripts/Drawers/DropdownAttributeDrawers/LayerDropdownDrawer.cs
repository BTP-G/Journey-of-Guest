using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;

namespace EditorAttributes.Editor {
    [CustomPropertyDrawer(typeof(LayerDropdownAttribute))]
    public class LayerDropdownDrawer : CollectionDisplayDrawer {
        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            if (!IsSupportedPropertyType(property)) {
                return new HelpBox("The LayerDropdown Attribute can only be attached to a string", HelpBoxMessageType.Error);
            }

            var layerNames = InternalEditorUtility.layers.ToList();
            var dropdownField = CreateDropdownField(layerNames, property);

            UpdateVisualElement(dropdownField, () => {
                var sceneNames = InternalEditorUtility.layers.ToList();

                if (IsCollectionValid(sceneNames)) {
                    dropdownField.choices = sceneNames;
                }
            });

            return dropdownField;
        }

        protected override void PasteValue(VisualElement element, SerializedProperty property, string clipboardValue) {
            var dropdown = element as DropdownField;

            if (dropdown.choices.Contains(clipboardValue)) {
                base.PasteValue(element, property, clipboardValue);
                dropdown.SetValueWithoutNotify(clipboardValue);
            } else {
                Debug.LogWarning($"Could not paste value <b>{clipboardValue}</b> since is not availiable as an option in the dropdown");
            }
        }

        protected override bool IsSupportedPropertyType(SerializedProperty property) {
            return property.propertyType == SerializedPropertyType.String;
        }
    }
}
