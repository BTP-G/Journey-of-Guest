using UnityEditor;
using UnityEditor.UIElements;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;

namespace TinyScript {
    [CustomEditor(typeof(LootDrop))]
    public class LootDropEditor : Editor {
        private ReorderableList _guaranteedList;
        private ReorderableList _oneFromList;
        private LootDrop ld;

        public override VisualElement CreateInspectorGUI() {
            var containerColor = EditorGUIUtility.isProSkin ? new Color(65 / 255f, 65 / 255f, 65 / 255f) : new Color(200 / 255f, 200 / 255f, 200 / 255f);
            var borderColor = EditorGUIUtility.isProSkin ? new Color(36f / 255f, 36f / 255f, 36f / 255f) : new Color(161 / 255f, 161 / 255f, 161 / 255f);
            ld = target as LootDrop;

            var root = new VisualElement();
            root.style.paddingTop = 4;

            var guaranteedProp = serializedObject.FindProperty("GuaranteedLootTable");
            var oneFromListProp = serializedObject.FindProperty("OneItemFromList");
            var weightToNoDropProp = serializedObject.FindProperty("WeightToNoDrop");

            var defCountProp = serializedObject.FindProperty("DefualtAdditionalDropCount");
            var defMinProp = serializedObject.FindProperty("DefualtMinDropRange");
            var defMaxProp = serializedObject.FindProperty("DefualtMaxDropRange");

            // Reorderable Lists
            root.Add(CreateReorderableSection("Guaranteed Loot", guaranteedProp, ref _guaranteedList));
            root.Add(CreateReorderableSection("Random Loot", oneFromListProp, ref _oneFromList));
            // No Drop Chance
            var noDropField = new PropertyField(weightToNoDropProp, "Weight for No Random Drop");
            noDropField.Bind(serializedObject);
            noDropField.style.marginRight = 3;
            root.Add(noDropField);

            // Drop Settings
            var dropSettingsHeader = CreateHeader("Defualt Drop Settings");
            dropSettingsHeader.style.marginTop = 8;
            root.Add(dropSettingsHeader);

            var defaultsBox = new VisualElement();
            defaultsBox.style.paddingRight = 5;
            defaultsBox.style.paddingBottom = 2;
            defaultsBox.style.backgroundColor = containerColor;
            defaultsBox.style.borderTopColor = borderColor;
            defaultsBox.style.borderBottomColor = borderColor;
            defaultsBox.style.borderRightColor = borderColor;
            defaultsBox.style.borderLeftColor = borderColor;
            defaultsBox.style.borderTopWidth = 1;
            defaultsBox.style.borderBottomWidth = 1;
            defaultsBox.style.borderRightWidth = 1;
            defaultsBox.style.borderLeftWidth = 1;
            defaultsBox.style.borderBottomLeftRadius = 4;
            defaultsBox.style.borderBottomRightRadius = 4;

            var helpBox = new HelpBox("...", HelpBoxMessageType.Info);
            helpBox.style.marginRight = -2;
            helpBox.style.marginBottom = 3;
            helpBox.style.marginLeft = 3;
            helpBox.style.marginTop = 3;
            defaultsBox.Add(helpBox);

            var defCountField = new PropertyField(defCountProp, "Additional Drop Count");
            var defMinField = new PropertyField(defMinProp, "Min Drop Range");
            var defMaxField = new PropertyField(defMaxProp, "Max Drop Range");
            defCountField.Bind(serializedObject);
            defMinField.Bind(serializedObject);
            defMaxField.Bind(serializedObject);
            defaultsBox.Add(defCountField);
            defaultsBox.Add(defMinField);
            defaultsBox.Add(defMaxField);

            root.Add(defaultsBox);

            // Drop Change Graph
            var dropInfoHeader = CreateHeader("Drop Change Visualization");
            dropInfoHeader.style.marginTop = 8;
            root.Add(dropInfoHeader);

            var dropInfoContainer = new VisualElement();
            dropInfoContainer.style.paddingRight = 5;
            dropInfoContainer.style.paddingBottom = 2;
            dropInfoContainer.style.backgroundColor = containerColor;
            dropInfoContainer.style.borderTopColor = borderColor;
            dropInfoContainer.style.borderBottomColor = borderColor;
            dropInfoContainer.style.borderRightColor = borderColor;
            dropInfoContainer.style.borderLeftColor = borderColor;
            dropInfoContainer.style.borderTopWidth = 1;
            dropInfoContainer.style.borderBottomWidth = 1;
            dropInfoContainer.style.borderRightWidth = 1;
            dropInfoContainer.style.borderLeftWidth = 1;
            dropInfoContainer.style.borderBottomLeftRadius = 4;
            dropInfoContainer.style.borderBottomRightRadius = 4;

            root.Add(dropInfoContainer);

            var totalWeight = ld.WeightToNoDrop;

            if (ld.OneItemFromList != null) {
                for (var j = 0; j < ld.OneItemFromList.Length; j++) {
                    totalWeight += ld.OneItemFromList[j].Weight;
                }
            }

            for (var i = 0; i < ld.GuaranteedLootTable.Length; i++) {
                dropInfoContainer.Add(DrawDropItemChange(ld.GuaranteedLootTable[i].Weight, ld.GuaranteedLootTable[i]));
            }
            for (var i = 0; i < ld.OneItemFromList.Length; i++) {
                dropInfoContainer.Add(DrawDropItemChange(totalWeight, ld.OneItemFromList[i]));
            }
            dropInfoContainer.Add(DrawDropItemChange(totalWeight, noDropChange: ld.WeightToNoDrop));

            return root;
        }

        private static VisualElement CreateReorderableSection(string headerText, SerializedProperty arrayProp, ref ReorderableList list) {
            var headerColor = EditorGUIUtility.isProSkin ? new Color(53f / 255f, 53f / 255f, 53f / 255f) : new Color(182 / 255f, 182 / 255f, 182 / 255f);
            var borderColor = EditorGUIUtility.isProSkin ? new Color(36f / 255f, 36f / 255f, 36f / 255f) : new Color(161 / 255f, 161 / 255f, 161 / 255f);

            if (list == null) {
                list = new ReorderableList(arrayProp.serializedObject, arrayProp, true, false, false, false) {
                    drawHeaderCallback = rect => EditorGUI.LabelField(rect, headerText),
                    elementHeightCallback = index => {
                        if (index < 0 || index >= arrayProp.arraySize) {
                            return EditorGUIUtility.singleLineHeight;
                        }

                        return EditorGUI.GetPropertyHeight(arrayProp.GetArrayElementAtIndex(index), true) + 4;
                    },
                    drawElementCallback = (rect, index, isActive, isFocused) => {
                        var element = arrayProp.GetArrayElementAtIndex(index);
                        rect.y += 2;
                        rect.height = EditorGUI.GetPropertyHeight(element, true);
                        EditorGUI.PropertyField(rect, element, GUIContent.none, true);
                    }
                };

                list.onAddCallback ??= l => ReorderableList.defaultBehaviours.DoAddButton(l);
                list.onRemoveCallback ??= l => ReorderableList.defaultBehaviours.DoRemoveButton(l);
                list.footerHeight = 4;
            }

            var box = new VisualElement();

            var boxHeader = CreateHeader(headerText);
            box.Add(boxHeader);

            var localList = list;

            var addButton = new Button(() => {
                localList.serializedProperty.serializedObject.Update();
                localList.onAddCallback?.Invoke(localList);
                localList.serializedProperty.serializedObject.ApplyModifiedProperties();
            }) {
                text = "+",
                style =
            {
                width = 24,
                height = 24,
                marginLeft = 0,
                marginRight = 0,
                marginTop = 0,
                marginBottom = 0,
                borderTopWidth = 0,
                borderBottomWidth = 0,
                borderRightWidth = 0,
                borderLeftWidth = 1,
                borderTopLeftRadius = 0,
                borderBottomLeftRadius = 0,
                fontSize = 20,
                borderLeftColor = borderColor,
                backgroundColor = Color.clear
            }
            };

            boxHeader.Add(addButton);

            var removeButton = new Button(() => {
                localList.serializedProperty.serializedObject.Update();
                localList.onRemoveCallback?.Invoke(localList);
                localList.serializedProperty.serializedObject.ApplyModifiedProperties();
            }) {
                text = "-",
                style =
            {
                width = 24,
                height = 24,
                marginLeft = 0,
                marginRight = 0,
                marginTop = 0,
                marginBottom = 0,
                borderTopWidth = 0,
                borderBottomWidth = 0,
                borderRightWidth = 0,
                borderLeftWidth = 1,
                borderTopLeftRadius = 0,
                borderBottomLeftRadius = 0,
                fontSize = 20,
                borderLeftColor = borderColor,
                backgroundColor = Color.clear
            }
            };

            boxHeader.Add(removeButton);

            var imgui = new IMGUIContainer(() => {
                localList.serializedProperty.serializedObject.Update();
                localList.DoLayoutList();
                localList.serializedProperty.serializedObject.ApplyModifiedProperties();
            });

            box.Add(imgui);
            return box;
        }

        private static VisualElement CreateHeader(string text) {
            var headerColor = EditorGUIUtility.isProSkin ? new Color(53f / 255f, 53f / 255f, 53f / 255f) : new Color(182 / 255f, 182 / 255f, 182 / 255f);
            var borderColor = EditorGUIUtility.isProSkin ? new Color(36f / 255f, 36f / 255f, 36f / 255f) : new Color(161 / 255f, 161 / 255f, 161 / 255f);

            var boxHeader = new VisualElement();
            boxHeader.style.backgroundColor = headerColor;
            boxHeader.style.borderTopColor = borderColor;
            boxHeader.style.borderBottomColor = borderColor;
            boxHeader.style.borderRightColor = borderColor;
            boxHeader.style.borderLeftColor = borderColor;
            boxHeader.style.borderTopWidth = 1;
            boxHeader.style.borderBottomWidth = 0;
            boxHeader.style.borderRightWidth = 1;
            boxHeader.style.borderLeftWidth = 1;
            boxHeader.style.height = 25;

            boxHeader.style.flexDirection = FlexDirection.Row;
            boxHeader.style.alignItems = Align.Center;

            //box.Add(boxHeader);

            var headerLabel = new Label(text);
            headerLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            headerLabel.style.flexGrow = 1;
            headerLabel.style.marginLeft = 6;
            boxHeader.Add(headerLabel);
            return boxHeader;
        }

        private static VisualElement DrawDropItemChange(float TotalWidth, DropChangeItem change = null, float noDropChange = 0) {
            Debug.Log(change == null);

            var containerColor = EditorGUIUtility.isProSkin ? new Color(65 / 255f, 65 / 255f, 65 / 255f) : new Color(200 / 255f, 200 / 255f, 200 / 255f);
            var borderColor = EditorGUIUtility.isProSkin ? new Color(36f / 255f, 36f / 255f, 36f / 255f) : new Color(161 / 255f, 161 / 255f, 161 / 255f);
            var progressBarColor = EditorGUIUtility.isProSkin ? new Color(40 / 255f, 40 / 255f, 40 / 255f) : new Color(161 / 255f, 161 / 255f, 161 / 255f);

            var container = new VisualElement();
            container.style.marginTop = 2;
            container.style.marginLeft = 2;
            container.style.marginRight = -3;
            container.style.backgroundColor = containerColor;
            container.style.borderTopColor = borderColor;
            container.style.borderBottomColor = borderColor;
            container.style.borderRightColor = borderColor;
            container.style.borderLeftColor = borderColor;
            container.style.borderTopWidth = 1;
            container.style.borderBottomWidth = 1;
            container.style.borderRightWidth = 1;
            container.style.borderLeftWidth = 1;
            container.style.borderTopLeftRadius = 3;
            container.style.borderTopRightRadius = 3;
            container.style.borderBottomLeftRadius = 3;
            container.style.borderBottomRightRadius = 3;

            var containerLabels = new VisualElement();
            containerLabels.style.flexDirection = FlexDirection.Row;
            containerLabels.style.marginTop = 2;
            containerLabels.style.marginLeft = 2;
            containerLabels.style.marginRight = 2;
            containerLabels.style.alignItems = Align.Center;
            container.Add(containerLabels);

            var itemLabel = new Label {
                text = change != null && change.Drop != null ? change.Drop.name : "No Drop"
            };
            itemLabel.style.flexGrow = 1;
            itemLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            containerLabels.Add(itemLabel);

            var countLabel = new Label();
            if (change != null) {
                countLabel.text = $"Can Drop: [{change.MinCountItem}-{change.MaxCountItem}]";
            }

            countLabel.style.fontSize = 10;
            countLabel.style.opacity = 0.7f;
            containerLabels.Add(countLabel);

            // Progress Bar
            var progressBar = new VisualElement();
            progressBar.style.height = 16;
            progressBar.style.marginLeft = 2;
            progressBar.style.marginRight = 2;
            progressBar.style.marginBottom = 2;
            progressBar.style.marginTop = 1;
            progressBar.style.borderTopLeftRadius = 3;
            progressBar.style.borderTopRightRadius = 3;
            progressBar.style.borderBottomLeftRadius = 3;
            progressBar.style.borderBottomRightRadius = 3;
            progressBar.style.borderTopColor = borderColor;
            progressBar.style.borderBottomColor = borderColor;
            progressBar.style.borderRightColor = borderColor;
            progressBar.style.borderLeftColor = borderColor;
            progressBar.style.borderTopWidth = 1;
            progressBar.style.borderBottomWidth = 1;
            progressBar.style.borderRightWidth = 1;
            progressBar.style.borderLeftWidth = 1;
            progressBar.style.backgroundColor = progressBarColor;

            var progressBarFill = new VisualElement();
            progressBarFill.style.backgroundColor = new Color(252f / 255f, 196f / 255f, 25f / 255f, 0.5f);
            progressBarFill.style.height = new StyleLength(new Length(100, LengthUnit.Percent));
            progressBarFill.style.width = new StyleLength(new Length(change != null ? change.Weight / TotalWidth * 100 : noDropChange / TotalWidth * 100, LengthUnit.Percent));
            progressBarFill.style.borderTopLeftRadius = 3;
            progressBarFill.style.borderTopRightRadius = 3;
            progressBarFill.style.borderBottomLeftRadius = 3;
            progressBarFill.style.borderBottomRightRadius = 3;

            var dropChangeText = new Label {
                text = change != null ? $"{change.Weight / TotalWidth * 100:0.00}%" : $"{noDropChange / TotalWidth * 100:0.00}%"
            };
            if (change != null && change.Weight == TotalWidth) {
                dropChangeText.text = "100.00% [Guaranteed Drop]";
            }

            dropChangeText.style.position = Position.Absolute;
            dropChangeText.style.left = 2;
            dropChangeText.style.right = 2;
            dropChangeText.style.top = 0;
            dropChangeText.style.bottom = 0;
            dropChangeText.style.unityTextAlign = TextAnchor.MiddleLeft;
            dropChangeText.style.unityFontStyleAndWeight = FontStyle.Bold;
            dropChangeText.style.fontSize = 10;

            progressBar.Add(progressBarFill);
            progressBar.Add(dropChangeText);
            if (change != null) {
                var weightStats = new Label($"[{change.Weight}/{TotalWidth}]");
                weightStats.style.position = Position.Absolute;
                weightStats.style.left = 2;
                weightStats.style.right = 2;
                weightStats.style.top = 0;
                weightStats.style.bottom = 0;
                weightStats.style.unityTextAlign = TextAnchor.MiddleRight;
                weightStats.style.opacity = 0.7f;
                weightStats.style.fontSize = 10;
                if (TotalWidth != change.Weight) {
                    progressBar.Add(weightStats);
                }
            }
            container.Add(progressBar);

            return container;
        }
    }
}
