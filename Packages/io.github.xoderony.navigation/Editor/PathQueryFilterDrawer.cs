using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.AI;
using UnityEngine.UIElements;

namespace Xoderony.AI.Editor {

    [CustomPropertyDrawer(typeof(PathQueryFilter))]
    public class PathQueryFilterDrawer : PropertyDrawer {

        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            var container = new VisualElement();

            // 创建 AgentTypeID 字段
            var agentTypeIDProperty = property.FindPropertyRelative("agentTypeID");
            container.Add(CreateAgentTypeDropdown(agentTypeIDProperty));

            // 创建 AreaMask 字段
            var areaMaskProperty = property.FindPropertyRelative("areaMask");
            container.Add(CreateAreaMaskField(areaMaskProperty));

            // 添加一些样式
            container.style.marginBottom = 5;
            container.style.marginTop = 5;

            return container;
        }

        private DropdownField CreateAgentTypeDropdown(SerializedProperty agentTypeIDProperty) {
            var settingsCount = NavMesh.GetSettingsCount();
            var choices = new List<string>(settingsCount);
            var indexToAgentTypeID = new int[settingsCount];
            string currentValue = null;
            for (var i = 0; i < settingsCount; i++) {
                choices.Add(i.ToString());
                var setting = NavMesh.GetSettingsByIndex(i);
                indexToAgentTypeID[i] = setting.agentTypeID;
                if (setting.agentTypeID == agentTypeIDProperty.intValue) {
                    currentValue = i.ToString();
                }
            }
            var dropdown = new DropdownField("Agent Type") {
                choices = choices,
                value = currentValue,
                formatListItemCallback = Format,
                formatSelectedValueCallback = Format,
            };
            dropdown.RegisterValueChangedCallback(evt => {
                if (int.TryParse(evt.newValue, out var index)) {
                    agentTypeIDProperty.intValue = indexToAgentTypeID[index];
                    agentTypeIDProperty.serializedObject.ApplyModifiedProperties();
                }
            });
            return dropdown;

            string Format(string value) {
                if (int.TryParse(value, out var index)) {
                    return NavMesh.GetSettingsNameFromID(indexToAgentTypeID[index]);
                }
                return "Invalid";
            }
        }

        private MaskField CreateAreaMaskField(SerializedProperty areaMaskProperty) {
            var areaNames = NavMesh.GetAreaNames();
            var choices = new List<string>(areaNames.Length);
            var choicesMasks = new List<int>(areaNames.Length);
            foreach (var areaName in areaNames) {
                choices.Add(areaName);
                choicesMasks.Add(1 << NavMesh.GetAreaFromName(areaName));
            }
            var maskField = new MaskField("Area Mask") {
                value = areaMaskProperty.intValue,
                choices = choices,
                choicesMasks = choicesMasks,
            };
            maskField.RegisterValueChangedCallback(evt => {
                areaMaskProperty.intValue = evt.newValue;
                areaMaskProperty.serializedObject.ApplyModifiedProperties();
            });
            return maskField;
        }
    }
}