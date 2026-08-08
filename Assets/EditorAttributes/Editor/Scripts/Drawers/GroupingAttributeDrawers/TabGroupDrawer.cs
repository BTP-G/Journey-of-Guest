using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;

namespace EditorAttributes.Editor {
    [CustomPropertyDrawer(typeof(TabGroupAttribute))]
    public class TabGroupDrawer : GroupDrawer {
        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            var tabGroupAttribute = attribute as TabGroupAttribute;

            var selectedTabSaveKey = CreatePropertySaveKey(property, "SelectedTab");
            var propertyNames = GetPropertyDisplayNames(property, tabGroupAttribute);

            TabView tabView = new() { selectedTabIndex = EditorPrefs.GetInt(selectedTabSaveKey) };
            tabView.activeTabChanged += (_, _) => EditorPrefs.SetInt(selectedTabSaveKey, tabView.selectedTabIndex);

            ApplyBoxStyle(tabView);

            for (var i = 0; i < propertyNames.Length; i++) {
                var propertyName = propertyNames[i];
                Tab tab = new(propertyName);

                ApplyBoxStyle(tab);

                var fieldName = tabGroupAttribute.FieldsToGroup[i];
                var groupProperty = CreateGroupProperty(fieldName, property);

                tab.Add(groupProperty);
                tabView.Add(tab);
            }

            return tabView;
        }

        private string[] GetPropertyDisplayNames(SerializedProperty property, TabGroupAttribute tabGroupAttribute) {
            List<string> stringList = new();

            foreach (var field in tabGroupAttribute.FieldsToGroup) {
                var fieldProperty = FindNestedProperty(property, GetSerializedPropertyName(field, property));

                stringList.Add(fieldProperty == null ? field : fieldProperty.displayName);
            }

            return stringList.ToArray();
        }
    }
}
