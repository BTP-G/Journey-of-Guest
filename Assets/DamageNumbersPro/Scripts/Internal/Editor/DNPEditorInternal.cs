#if UNITY_EDITOR

using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace DamageNumbersPro.Internal {
    public static class DNPEditorInternal {
        // Public
        public static int currentTab;
        public static bool repaintViews;
        public static HashSet<string> hints;
        public static DamageNumber[] damageNumbers;
        public static TextMeshPro[] textMeshPros;

        // Private
        private static Transform[] meshAs;
        private static Transform[] meshBs;

        // Presets
        private static Dictionary<string, DNPPreset[]> allPresets;

        // GUI Resources
        public static GUIStyle labelStyle;
        public static GUIStyle buttonStyle;
        public static GUIStyle rightTextStyle;
        public static GUIStyle centerTextStyle;
        public static GUIStyle bottomRightTextStyle;
        public static GUIStyle topRightTextStyle;
        private static GUIStyle whiteBoxStyle;
        private static Texture2D whiteBoxTexture;
        private static Texture bannerTexture;
        public static float currentWidth;

        // External Editor
        private static int currentEditor;
        private static bool cleanEditor;
        private static Material[] currentMaterials;
        private static MaterialEditor materialEditor;
        private static Editor textMeshProEditor;
        private static bool generatedGUIStyles;

        public static void PrepareInspector(DamageNumberEditor damageNumberEditor) {
            // Clean Editors
            cleanEditor = true;
            generatedGUIStyles = false;

            // Get Damage Numbers
            damageNumbers = new DamageNumber[damageNumberEditor.targets.Length];
            for (var i = 0; i < damageNumberEditor.targets.Length; i++) {
                damageNumbers[i] = (DamageNumber)damageNumberEditor.targets[i];
            }

            // Type
            var isMesh = damageNumbers[0].IsMesh();

            // Get Presets
            allPresets = new Dictionary<string, DNPPreset[]> {
                { "Style", Resources.LoadAll<DNPPreset>("DNP/Style") },
                { "Fade In", Resources.LoadAll<DNPPreset>("DNP/Fade In") },
                { "Fade Out", Resources.LoadAll<DNPPreset>("DNP/Fade Out") },
                { "Behaviour", Resources.LoadAll<DNPPreset>("DNP/Behaviour") }
            };

            // Get Structural Objects
            textMeshPros = new TextMeshPro[damageNumbers.Length];
            meshAs = new Transform[damageNumbers.Length];
            meshBs = new Transform[damageNumbers.Length];
            if (isMesh) {
                for (var n = 0; n < damageNumbers.Length; n++) {
                    var dnTransform = damageNumbers[n].transform;

                    // TMP
                    var textMeshProTransform = dnTransform.Find("TMP");
                    if (textMeshProTransform != null) {
                        textMeshPros[n] = textMeshProTransform.GetComponent<TextMeshPro>();
                    }

                    // MeshA
                    meshAs[n] = dnTransform.Find("MeshA");

                    // MeshB
                    meshBs[n] = dnTransform.Find("MeshB");
                }
            }

            // Get Banner Texture
            if (damageNumbers != null && damageNumbers.Length > 0 && damageNumbers[0] != null) {
                bannerTexture = Resources.Load<Texture>("DNP/Textures/DNP_Banner");
            }

            // Create White Box Texture
            var pixels = new Color[4];
            for (var n = 0; n < pixels.Length; n++) {
                pixels[n] = Color.white;
            }
            whiteBoxTexture = new Texture2D(2, 2);
            whiteBoxTexture.SetPixels(pixels);
            whiteBoxTexture.Apply();

            // Close Hints
            hints = new HashSet<string>();
        }

        public static void OnInspectorGUI(DamageNumberEditor damageNumberEditor) {
            // Prepare Inspector
            if (damageNumbers == null || damageNumbers.Length == 0 || damageNumbers[0] == null) {
                PrepareInspector(damageNumberEditor);
            }

            // Prepare Styles
            PrepareStyles();

            // Repaint
            if (repaintViews) {
                repaintViews = false;
                UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
            }
        }

        #region Inspector Top
        public static void DrawInspectorTop(bool isMesh) {
            // Banner
            var bannerRect = DrawBanner(isMesh);

            // Tabs
            DrawTabs(bannerRect);

            // Distance
            EditorGUILayout.Space(42);
        }
        private static Rect DrawBanner(bool isMesh) {
            Rect bannerRect = default;
            if (bannerTexture != null) {
                // Banner
                EditorGUILayout.BeginVertical();
                GUILayout.Label("", GUILayout.Height(100 + (0.38f * Mathf.Min(0, EditorGUIUtility.currentViewWidth - 430))));
                bannerRect = GUILayoutUtility.GetLastRect();
                var ratio = bannerRect.width / bannerRect.height / 8f;
                var clipRect = new Rect((1 - ratio) * 0.5f, 0, ratio, 1);
                GUI.DrawTextureWithTexCoords(bannerRect, bannerTexture, clipRect, true);
                EditorGUILayout.EndVertical();

                // Version Info
                var infoRect = new Rect(bannerRect);
                infoRect.width -= 3;
                infoRect.y += 4;
                infoRect.height -= 8;
                GUI.Label(infoRect, "<color=#FFFFFF><b><size=10>v</size>" + DamageNumberEditor.version + " </b></color>", bottomRightTextStyle);

                // Type Info
                GUI.Label(infoRect, "<color=#FFFFFF><b>" + (isMesh ? "Mesh" : "GUI") + "<size=10> </size></b></color>", topRightTextStyle);

                // Button
                var documentationRect = GUILayoutUtility.GetLastRect();
                documentationRect.x += 9;
                documentationRect.y += 9;
                documentationRect.width = 56;
                documentationRect.height = 18;
                if (GUI.Button(documentationRect, "<b>Manual</b>", buttonStyle)) {
                    Application.OpenURL("https://ekincantas.com/damage-numbers-pro/");
                }
                documentationRect.y += 21;
                if (GUI.Button(documentationRect, "<b>Discord</b>", buttonStyle)) {
                    Application.OpenURL("https://discord.gg/nWbRkN8Zxr");
                }

                // Box
                BoxLastRect();

                EditorGUILayout.Space();

                // Calculate Width for GUI Scaling
                var newWidth = GUILayoutUtility.GetLastRect().width;
                if (newWidth > 50) {
                    currentWidth = newWidth;
                }
            }

            return bannerRect;
        }
        private static void DrawTabs(Rect lastRect) {
            // Position
            lastRect.y += lastRect.height - 3;
            lastRect.height = 29;

            // Row 1
            var lastTab = currentTab;

            currentTab = GUI.Toolbar(lastRect, currentTab, new string[] { "Main", "Text", "Fade In", "Fade Out" }, buttonStyle);
            lastRect.height -= 1;
            BoxRect(lastRect);

            var rotAndScaleText = "Rotation & Size";
            if (currentWidth < 388) {
                rotAndScaleText = "<size=11>Rotation & Size</size>";

                if (currentWidth < 356) {
                    rotAndScaleText = "<size=10>Rotation & Size</size>";

                    if (currentWidth < 324) {
                        rotAndScaleText = "<size=9>Rotation & Size</size>";

                        if (currentWidth < 303) {
                            rotAndScaleText = "<size=8>Rotation & Size</size>";

                            if (currentWidth < 276) {
                                rotAndScaleText = "<size=7>Rotation & Size</size>";
                            }
                        }
                    }
                }
            }

            var spamText = "Spam Control";
            if (currentWidth < 340) {
                spamText = "<size=11>Spam Control</size>";

                if (currentWidth < 324) {
                    spamText = "<size=10>Spam Control</size>";

                    if (currentWidth < 304) {
                        spamText = "<size=9>Spam Control</size>";

                        if (currentWidth < 367) {
                            spamText = "<size=8>Spam Control</size>";
                        }
                    }
                }
            }

            var performanceText = "Performance";
            if (currentWidth < 336) {
                performanceText = "<size=11>Performance</size>";

                if (currentWidth < 310) {
                    performanceText = "<size=10>Performance</size>";

                    if (currentWidth < 292) {
                        performanceText = "<size=9>Performance</size>";
                    }
                }
            }

            var movementText = "Movement";
            if (currentWidth < 293) {
                movementText = "<size=11>Movement</size>";

                if (currentWidth < 282) {
                    movementText = "<size=10>Movement</size>";

                    if (currentWidth < 263) {
                        movementText = "<size=9>Movement</size>";
                    }
                }
            }

            // Row 2
            lastRect.y += lastRect.height - 3;
            lastRect.height = 22;
            currentTab = 4 + GUI.Toolbar(lastRect, currentTab - 4, new string[] { movementText, rotAndScaleText, spamText, performanceText }, buttonStyle);
            lastRect.height += 2;

            // On Switch
            if (currentTab != lastTab) {
                EditorGUIUtility.keyboardControl = EditorGUIUtility.hotControl = 0;
                hints = new HashSet<string>(); // Clear Hints
            }

            // Box
            BoxRect(lastRect, true, false);
        }
        #endregion

        #region Special Sections
        public static void FinalInformation() {
            var finalInformationColor = new Color(0.93f, 0.95f, 1);

            var linkStyle = new GUIStyle(labelStyle);
            linkStyle.normal.textColor = linkStyle.focused.textColor = linkStyle.hover.textColor = EditorGUIUtility.isProSkin ? new Color(0.8f, 0.9f, 1f, 1) : new Color(0.1f, 0.2f, 0.4f, 1);
            linkStyle.active.textColor = EditorGUIUtility.isProSkin ? new Color(0.6f, 0.8f, 1f, 1) : new Color(0.15f, 0.4f, 0.6f, 1);

            EditorGUILayout.Space(2);
            StartBox(finalInformationColor);
            EditorGUILayout.BeginVertical();

            GUI.color = new Color(1, 1, 1f, 0.75f);
            if (currentWidth < 285) {
                if (currentWidth < 265) {
                    Label("<size=10><b>Thank you for using Damage Numbers Pro.</b></size>");
                } else {
                    Label("<size=11><b>Thank you for using Damage Numbers Pro.</b></size>");
                }
            } else {
                Label("<b>Thank you for using Damage Numbers Pro.</b>");
            }
            Label("<b>Contact me if you need any help.</b>");
            EditorGUILayout.Space(8);

            EditorGUILayout.BeginHorizontal();
            GUI.color = new Color(1, 1, 1, 1);

            // Link Shrinking
            var docLink = "https://ekincantas.com/damage-numbers-pro/";
            if (currentWidth < 420f) {
                docLink = "https://ekincantas.com/damage-numbers...";

                if (currentWidth < 398) {
                    docLink = "https://ekincantas.com/damage...";

                    if (currentWidth < 340) {
                        docLink = "https://ekincantas.com/...";

                        if (currentWidth < 293) {
                            docLink = "Open Link";
                        }
                    }
                }
            }
            var discordLink = "https://discord.gg/nWbRkN8Zxr";
            if (currentWidth < 335f) {
                discordLink = "https://discord.gg/...";

                if (currentWidth < 264) {
                    discordLink = "Open Link";
                }
            }

            EditorGUILayout.LabelField("<b>Documentation:</b>", labelStyle, GUILayout.Width(100));
            if (GUILayout.Button("<b>" + docLink + "</b>", linkStyle)) {
                Application.OpenURL("https://ekincantas.com/damage-numbers-pro/");
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUI.color = new Color(1, 1, 1, 1);
            EditorGUILayout.LabelField("<b>Discord:</b>", labelStyle, GUILayout.Width(100));
            if (GUILayout.Button("<b>" + discordLink + "</b>", linkStyle)) {
                Application.OpenURL("https://discord.gg/nWbRkN8Zxr");
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUI.color = new Color(1, 1, 1, 1);

            var emailPrefix = "<b>Email:</b>";
            if (currentWidth < 259) {
                emailPrefix = "<size=11><b>Email:</b></size>";

                if (currentWidth < 256) {
                    emailPrefix = "<size=10><b>Email:</b></size>";

                    if (currentWidth < 254) {
                        emailPrefix = "<size=9><b>Email:</b></size>";
                    }
                }
            }

            EditorGUILayout.LabelField(emailPrefix, labelStyle, GUILayout.Width(100 + Mathf.Min(0, currentWidth - 320)));
            EditorGUILayout.SelectableLabel("<b>ekincantascontact@gmail.com</b>", linkStyle, GUILayout.Height(16));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
            GUI.backgroundColor = Color.white;
            CloseBox(finalInformationColor);
            EditorGUILayout.Space(5);
        }

        public static void Externalnspectors(bool isMesh, Object target) {
            EditorGUILayout.Space(2);
            Lines();

            var externalInspectorColor = new Color(0.93f, 0.95f, 1);

            EditorGUILayout.Space(2);
            StartBox(externalInspectorColor);
            GUI.backgroundColor = externalInspectorColor;
            EditorGUILayout.BeginVertical();

            var editingPrefabPreview = EditingPrefabPreview(target);

            if (editingPrefabPreview) {
                GUI.color = new Color(1, 1, 1f, 0.75f);
                ScalingLabel("<b>Open</b> the prefab to access the <b>presets</b>, <b>material</b> and <b>text mesh pro</b> tabs.", 440);
                OpenPrefabButton(target);

                GUI.backgroundColor = Color.white;
                EditorGUILayout.EndVertical();
                CloseBox(externalInspectorColor);
                return;
            }

            var previousEditor = currentEditor;
            if (cleanEditor) {
                previousEditor = -1; // Force Clean
                cleanEditor = false;
            }

            // Tab Names
            var textMeshProTab = "TextMeshPro";
            var materialTab = "Material";
            if (damageNumbers != null && damageNumbers.Length > 1) {
                textMeshProTab = damageNumbers.Length + " TextMeshPros";
                materialTab = "Materials";
            }
            if (currentWidth < 266) {
                textMeshProTab = "<size=11>" + textMeshProTab + "</size>";
            }

            // Tab Rect
            GUILayout.BeginVertical();
            GUILayout.Space(13);
            GUILayout.EndVertical();

            var tabRect = GUILayoutUtility.GetLastRect();
            tabRect.x -= 5;
            tabRect.width += 12;
            tabRect.y -= 5;
            tabRect.height = 25;

            // Draw Tab
            currentEditor = GUI.Toolbar(tabRect, currentEditor, new string[] { "Presets", materialTab, textMeshProTab });

            // Box Tab Rect
            tabRect.y += 23;
            tabRect.height = 3;
            tabRect.width -= 3;

            if (currentEditor == 1) {
                EditorGUILayout.Space(14);

                // Material
                if (previousEditor != 1) {
                    ResetMaterials();
                }

                if (materialEditor != null) {
                    materialEditor.DrawHeader();
                    materialEditor.OnInspectorGUI();
                }
            } else if (currentEditor == 2) {
                EditorGUILayout.Space(14);

                if (damageNumbers.Length > 1) {
                    GUI.color = new Color(1, 1, 1, 0.7f);
                    Label("The fancy inspector does not work for <b>multiple</b> damage numbers.");
                    Label("You can also <b>manually select</b> the text-mesh-pro components.");
                    Label("- Sorry for the inconvenience.");
                    GUI.color = Color.white;
                    EditorGUILayout.Space(8);
                }

                if (isMesh) {
                    // Text Mesh Pro
                    if (previousEditor != 2) {
                        if (textMeshProEditor != null) {
                            Object.DestroyImmediate(textMeshProEditor);
                        }
                        textMeshProEditor = Editor.CreateEditor(textMeshPros, null);
                    }

                    // Editor
                    if (textMeshProEditor != null) {
                        textMeshProEditor.DrawHeader();
                        if (textMeshPros.Length > 1) {
                            textMeshProEditor.DrawDefaultInspector();
                        } else {
                            textMeshProEditor.OnInspectorGUI();
                        }
                    }
                } else {
                    // Text Mesh Pro
                    if (previousEditor != 2) {
                        if (textMeshProEditor != null) {
                            Object.DestroyImmediate(textMeshProEditor);
                        }

                        var tmps = new TextMeshProUGUI[damageNumbers.Length];
                        for (var i = 0; i < damageNumbers.Length; i++) {
                            var tmpText = damageNumbers[i].GetTextMesh();

                            if (tmpText.GetType() == typeof(TextMeshProUGUI)) {
                                tmps[i] = (TextMeshProUGUI)tmpText;
                            } else {
                                return;
                            }
                        }

                        textMeshProEditor = Editor.CreateEditor(tmps, null);
                    }

                    // Editor
                    if (textMeshProEditor != null) {
                        textMeshProEditor.DrawHeader();
                        if (textMeshPros.Length > 1) {
                            textMeshProEditor.DrawDefaultInspector();
                        } else {
                            textMeshProEditor.OnInspectorGUI();
                        }

                        // Match both TMPs
                        foreach (var dn in damageNumbers) {
                            var tmps = dn.GetTextMeshs();
                            if (tmps.Length > 1) {
                                EditorUtility.CopySerialized((TextMeshProUGUI)tmps[0], (TextMeshProUGUI)tmps[1]);
                            }
                        }
                    }
                }
            } else {
                EditorGUILayout.Space(14);
                ShowPresets(isMesh);
            }

            EditorGUILayout.EndVertical();
            GUI.backgroundColor = Color.white;
            CloseBox(externalInspectorColor);

            // Line under Tabs
            GUI.color = externalInspectorColor;
            DrawOutlineBox(tabRect);
            tabRect.width += 3;
            DrawBox(tabRect);
            GUI.color = Color.white;
        }
        public static void ResetMaterials() {
            var allMaterials = new List<Material>();
            foreach (var dn in damageNumbers) {
                dn.GetReferencesIfNecessary();

                foreach (var mat in dn.GetSharedMaterials()) {
                    if (allMaterials.Contains(mat) == false) {
                        allMaterials.Add(mat);
                    }
                }
            }

            currentMaterials = new Material[allMaterials.Count];
            for (var n = 0; n < allMaterials.Count; n++) {
                currentMaterials[n] = allMaterials[n];
            }

            if (materialEditor != null) {
                Object.DestroyImmediate(materialEditor);
            }

            materialEditor = (MaterialEditor)Editor.CreateEditor(currentMaterials);
        }

        private static void ShowPresets(bool isMesh) {
            PresetCategory("Style", isMesh);
            EditorGUILayout.Space(12);
            PresetCategory("Fade In", isMesh);
            EditorGUILayout.Space(12);
            PresetCategory("Fade Out", isMesh);
            EditorGUILayout.Space(12);
            PresetCategory("Behaviour", isMesh);
        }
        private static void PresetCategory(string category, bool isMesh) {
            GUI.color = new Color(1, 1, 1, 0.7f);
            EditorGUILayout.LabelField("<size=14><b> - - - - - - - - - - - - - - - - - - - - - - - - - - - - - " + category + " - - - - - - - - - - - - - - - - - - - - - - - - - - - - - </b></size>", centerTextStyle);
            GUI.color = Color.white;

            var presets = allPresets[category];

            if (presets == null || presets.Length == 0) {
                GUI.color = new Color(1, 1, 1, 0.7f);
                Label("Presets could not be loaded.");
                Label("Maybe you deleted or moved a folder ?");
                GUI.color = Color.white;
                return;
            }

            var buttonsPerRow = 4;
            if (EditorGUIUtility.currentViewWidth < 440) {
                buttonsPerRow = 3;

                if (EditorGUIUtility.currentViewWidth < 375) {
                    buttonsPerRow = 2;
                }
            }

            var currentCount = 0;
            foreach (var preset in presets) {
                // Check Applied
                var isApplied = true;
                foreach (var dn in damageNumbers) {
                    if (!preset.IsApplied(dn)) {
                        isApplied = false;
                        break;
                    }
                }
                if (isApplied) {
                    GUI.enabled = false;
                }

                // Increase Count
                currentCount++;
                if (currentCount % buttonsPerRow == 1) {
                    if (currentCount > 1) {
                        EditorGUILayout.EndHorizontal();
                    }

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(5);
                }

                // Apply Button
                if (GUILayout.Button(preset.name)) {
                    if (isMesh) {
                        var undoObjects = new Object[damageNumbers.Length + textMeshPros.Length];
                        for (var i = 0; i < damageNumbers.Length; i++) {
                            undoObjects[i] = damageNumbers[i];
                        }
                        for (var i = 0; i < textMeshPros.Length; i++) {
                            undoObjects[i + damageNumbers.Length] = textMeshPros[i];
                        }

                        Undo.RecordObjects(undoObjects, "Applied the [" + preset.name + "] " + category + " Preset.");
                    } else {
                        var undoObjects = new Object[damageNumbers.Length * 3];
                        for (var i = 0; i < damageNumbers.Length; i += 3) {
                            undoObjects[i] = damageNumbers[i];
                            undoObjects[i + 1] = damageNumbers[i].GetTextMeshs()[0];
                            undoObjects[i + 2] = damageNumbers[i].GetTextMeshs()[1];
                        }

                        Undo.RecordObjects(undoObjects, "Applied the [" + preset.name + "] " + category + " Preset.");
                    }

                    foreach (var dn in damageNumbers) {
                        preset.Apply(dn);
                    }

                    foreach (var dn in damageNumbers) {
                        dn.UpdateText();
                    }
                }

                // Reenable GUI
                GUI.enabled = true;
            }

            GUI.enabled = false;
            GUI.color = new Color(0, 0, 0, 0);
            var modulo = currentCount % buttonsPerRow;
            if (modulo > 0) {
                for (var n = 0; n < buttonsPerRow - modulo; n++) {
                    GUILayout.Button("- - - - -");
                }
            }
            GUI.color = Color.white;
            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();
        }
        #endregion

        #region Structure
        public static bool CheckStructure(DamageNumberEditor damageNumberEditor) {
            // Type
            var isMesh = damageNumbers[0].IsMesh(); ;

            // Check if structure is flawed
            var isStructureFlawed = false;
            var isOutdated = false;
            if (isMesh) {
                for (var n = 0; n < damageNumbers.Length; n++) {
                    var dn = damageNumbers[n];
                    var tmp = textMeshPros[n];
                    var meshA = meshAs[n];
                    var meshB = meshBs[n];

                    if (tmp == null || meshA == null || meshB == null) {
                        isStructureFlawed = true;

                        if (dn.transform.Find("TextA") != null) {
                            isOutdated = true;
                        }

                        break;
                    }
                }
            } else {
                for (var n = 0; n < damageNumbers.Length; n++) {
                    var dn = damageNumbers[n];

                    if (dn.transform.Find("TMPA") == null || dn.transform.Find("TMPB") == null) {
                        isStructureFlawed = true;

                        break;
                    }
                }
            }

            // Create button to fix structure
            if (isStructureFlawed) {
                // Start Box
                StartBox(new Color(1, 1, 0.8f));

                // Structure Build Button
                GUI.color = new Color(1, 1, 0.8f);
                if (GUILayout.Button(isOutdated ? "Upgrade Structure" : "Build Structure", GUILayout.Width(140))) {
                    if (isMesh) {
                        foreach (var dn in damageNumbers) {
                            PrepareMeshStructure(dn.gameObject);
                        }
                    } else {
                        foreach (var dn in damageNumbers) {
                            PrepareGUIStructure(dn.gameObject);
                        }
                    }

                    PrepareInspector(damageNumberEditor);
                }

                // Text
                GUI.color = new Color(1, 1, 1, 0.7f);
                if (isOutdated) {
                    EditorGUILayout.LabelField("Version 4.0 has changed the structure of damage numbers.", labelStyle);
                    EditorGUILayout.LabelField("Click the button above to <b>upgrade</b> this damage number.", labelStyle);
                } else {
                    EditorGUILayout.LabelField("Important components are missing.", labelStyle);
                    EditorGUILayout.LabelField("Click the button above to <b>prepare</b> this damage number.", labelStyle);
                }

                // Close Box
                CloseBox(new Color(1, 1, 0.7f));
                EditorGUILayout.Space();
                return true;
            } else {
                return false;
            }
        }
        public static void PrepareMeshStructure(GameObject go) {
            // Add Sorting Group
            if (go.GetComponent<SortingGroup>() == null) {
                go.AddComponent<SortingGroup>().sortingOrder = 1000;
            }

            // Rename TextA to TMP
            var textA = go.transform.Find("TextA");
            if (textA != null) {
                textA.gameObject.name = "TMP";
            }

            // Destroy TextB
            if (go.transform.Find("TextB")) {
                MonoBehaviour.DestroyImmediate(go.transform.Find("TextB").gameObject, true);
            }

            // Create TMP
            if (go.transform.Find("TMP") == null) {
                NewTextMesh("TMP", go.transform);
            }

            // Create MeshA
            if (go.transform.Find("MeshA") == null) {
                DamageNumber.NewMesh("MeshA", go.transform);
            }

            // Create MeshB
            if (go.transform.Find("MeshB") == null) {
                DamageNumber.NewMesh("MeshB", go.transform);
            }

            // Undo
            Undo.RegisterCreatedObjectUndo(go, "Create new Damage Number (Mesh).");
        }
        public static void PrepareGUIStructure(GameObject go) {
            // Create TMP
            if (go.transform.Find("TMPA") == null) {
                NewTextGUI("TMPA", go.transform);
            }
            if (go.transform.Find("TMPB") == null) {
                NewTextGUI("TMPB", go.transform);
            }

            // Add Rect Component
            if (go.GetComponent<RectTransform>() == null) {
                go.AddComponent<RectTransform>();
            }

            // Undo
            Undo.RegisterCreatedObjectUndo(go, "Create new Damage Number (GUI).");
        }

        public static GameObject NewTextMesh(string tmName, Transform parent) {
            // GameObject
            var newTM = new GameObject {
                name = tmName
            };

            // TextMeshPro
            var tmp = newTM.AddComponent<TextMeshPro>();
            tmp.fontSize = 5;
            tmp.verticalAlignment = VerticalAlignmentOptions.Middle;
            tmp.horizontalAlignment = HorizontalAlignmentOptions.Center;
            tmp.text = "1";
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            tmp.font = Resources.Load<DNPPreset>("DNP/Style/Basic/Basic Default").fontAsset;

            // Size Delta
            var rectTransform = tmp.GetComponent<RectTransform>();
            if (rectTransform != null) {
                rectTransform.sizeDelta = new Vector2(4, 2);
            }

            // Transform
            newTM.transform.SetParent(parent, true);
            newTM.transform.localPosition = Vector3.zero;
            newTM.transform.localScale = Vector3.one;
            newTM.transform.localEulerAngles = Vector3.zero;

            return newTM;
        }

        public static GameObject NewTextGUI(string tmName, Transform parent) {
            // GameObject
            var newTM = new GameObject {
                name = tmName
            };

            // TextMeshPro
            var tmp = newTM.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = 30;
            tmp.verticalAlignment = VerticalAlignmentOptions.Middle;
            tmp.horizontalAlignment = HorizontalAlignmentOptions.Center;
            tmp.text = "1";
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            tmp.raycastTarget = false;

            // Size Delta
            var rectTransform = tmp.GetComponent<RectTransform>();
            if (rectTransform != null) {
                rectTransform.sizeDelta = new Vector2(4, 2);
            }

            // Transform
            newTM.transform.SetParent(parent, true);
            newTM.transform.localPosition = Vector3.zero;
            newTM.transform.localScale = Vector3.one;
            newTM.transform.localEulerAngles = Vector3.zero;

            return newTM;
        }
        public static void FixTextMeshPro() {
            var destroyedSomething = false;
            foreach (var textMesh in textMeshPros) {
                var tmp = textMesh.transform;
                tmp.localPosition = Vector3.zero;

                tmp.gameObject.SetActive(true);
                for (var n = 0; n < tmp.childCount; n++) {
                    DestroyOrDisable(tmp.GetChild(n).gameObject);
                    destroyedSomething = true;
                }
                tmp.gameObject.SetActive(false);
            }

            foreach (var meshA in meshAs) {
                for (var n = 0; n < meshA.childCount; n++) {
                    var child = meshA.GetChild(n);
                    if (child.GetComponent<MeshRenderer>() != null) {
                        destroyedSomething = true;
                        DestroyOrDisable(child.gameObject);
                    }
                }
            }

            foreach (var meshB in meshBs) {
                for (var n = 0; n < meshB.childCount; n++) {
                    var child = meshB.GetChild(n);
                    if (child.GetComponent<MeshRenderer>() != null) {
                        destroyedSomething = true;
                        DestroyOrDisable(child.gameObject);
                    }
                }
            }

            if (destroyedSomething) {
                foreach (var dn in damageNumbers) {
                    dn.GetReferences();
                }
            }
        }

        private static void DestroyOrDisable(GameObject go) {
            if (PrefabUtility.IsPartOfNonAssetPrefabInstance(go)) {
                go.SetActive(false);
            } else {
                Object.DestroyImmediate(go);
            }
        }

        #endregion

        #region Miscellaneous
        public static void BeginInspector() {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
        }
        public static void EndInspector() {
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(2);
            EditorGUILayout.EndHorizontal();
        }
        private static void PrepareStyles() {
            if (generatedGUIStyles) {
                return;
            }

            // Label
            labelStyle = new GUIStyle(GUI.skin.label) {
                richText = true
            };

            // Button
            buttonStyle = new GUIStyle(GUI.skin.button) {
                richText = true,
                alignment = TextAnchor.MiddleCenter
            };

            // Right Anchor
            rightTextStyle = new GUIStyle(labelStyle) {
                alignment = TextAnchor.MiddleRight
            };

            // Center Anchor
            centerTextStyle = new GUIStyle(labelStyle) {
                alignment = TextAnchor.MiddleCenter
            };

            // Bottom Right Anchor
            bottomRightTextStyle = new GUIStyle(labelStyle) {
                alignment = TextAnchor.LowerRight
            };

            // Top Right Anchor
            topRightTextStyle = new GUIStyle(labelStyle) {
                alignment = TextAnchor.UpperRight
            };

            // White Box
            whiteBoxStyle = new GUIStyle(GUI.skin.box);
            whiteBoxStyle.normal.background = whiteBoxStyle.onNormal.background = whiteBoxStyle.active.background =
            whiteBoxStyle.onActive.background = whiteBoxStyle.focused.background = whiteBoxStyle.onFocused.background =
            whiteBoxStyle.hover.background = whiteBoxStyle.onHover.background = whiteBoxTexture;

            // Rich Everything
            for (var n = 0; n < GUI.skin.customStyles.Length; n++) {
                if (GUI.skin.customStyles[n] != null) {
                    GUI.skin.customStyles[n].richText = true;
                }
            }

            // Finish
            generatedGUIStyles = true;
        }
        public static bool HintButton(string category) {
            var toggled = hints.Contains(category);

            var buttonName = toggled ? "<size=8> </size><b>?</b>" : "?";

            if (GUILayout.Button(buttonName, buttonStyle, GUILayout.Width(21))) {
                if (hints.Contains(category)) {
                    hints.Remove(category);
                } else {
                    hints.Add(category);
                }

                toggled = !toggled;
            }

            if (toggled) {
                GUI.color = new Color(1, 1, 1, 0.8f);
                GUI.Toolbar(GUILayoutUtility.GetLastRect(), 0, new string[] { buttonName }, buttonStyle);
                GUI.color = new Color(1, 1, 1, 1f);
            }

            return toggled;
        }
        public static void RepaintInspector() {
            repaintViews = true;
            GUI.FocusControl("");
        }

        public static bool EditingPrefabPreview(Object target) {
            return EditorUtility.IsPersistent(target) && PrefabStageUtility.GetCurrentPrefabStage() == null;
        }

        public static void OpenPrefabButton(Object target) {
            GUI.color = Color.white;
            if (GUILayout.Button("Click here to open the prefab.")) {
                try {
                    PrefabStageUtility.OpenPrefab(AssetDatabase.GetAssetPath(target));
                } catch {
                    EditorUtility.DisplayDialog("Something went wrong.", "Please open the prefab manually, by double-clicking it in the project window.", "Okay");
                }
            }
        }
        #endregion

        #region Label Utility
        public static void Lines() {
            GUI.color = new Color(1, 1, 1, 0.5f);
            EditorGUILayout.LabelField("− − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − − −", GUILayout.Height(7f));
            GUI.color = new Color(1, 1, 1, 1f);
        }
        public static void Label(string text) {
            EditorGUILayout.LabelField(text, labelStyle);
        }

        public static void ScalingLabel(string text, float scaleWidth) {
            var size = Mathf.FloorToInt(Mathf.Clamp(12 * Mathf.Pow(DNPEditorInternal.currentWidth / scaleWidth, 1.525f), 9, 12));
            Label("<size=" + size + ">" + text + "</size>");
        }

        public static string CheckmarkString(bool state) {
            if (EditorGUIUtility.isProSkin) {
                return state ? "<size=15><b><color=#00FF00>✓</color></b></size>" : "<size=16><b><color=#FF0000>✗</color></b></size>";
            } else {
                return state ? "<size=15><b><color=#00BB00>✓</color></b></size>" : "<size=16><b><color=#BB0000>✗</color></b></size>";
            }
        }
        #endregion

        #region Box Utility
        public static void StartBox(Color color, bool isActivated = true) {
            GUI.color = color;
            StartBox(isActivated);
            GUI.color = Color.white;
        }
        public static void StartBox(bool isActivated = true) {
            // Start Box
            if (EditorGUIUtility.isProSkin) {
                if (isActivated) {
                    GUI.backgroundColor = new Color(0.3f, 0.3f, 0.3f, 1);
                } else {
                    GUI.backgroundColor = new Color(0.25f, 0.25f, 0.25f, 1);
                }
            } else {
                if (isActivated) {
                    GUI.backgroundColor = new Color(0.68f, 0.68f, 0.68f, 1);
                } else {
                    GUI.backgroundColor = new Color(0.72f, 0.72f, 0.72f, 1);
                }
            }
            GUILayout.BeginHorizontal(whiteBoxStyle);
            GUILayout.Space(5);
            GUILayout.BeginVertical();
            GUILayout.Space(5);
            GUI.backgroundColor = Color.white;
        }
        public static void CloseBox(Color color, bool isActivated = true) {
            GUI.color = color;
            CloseBox(isActivated);
            GUI.color = Color.white;
        }
        public static void CloseBox(bool isActivated = true) {
            GUILayout.Space(5);
            EditorGUILayout.EndVertical();
            GUILayout.Space(5);
            EditorGUILayout.EndHorizontal();
            var lastRect = GUILayoutUtility.GetLastRect();
            lastRect.width -= 2;
            BoxLastRect(isActivated);
        }
        private static void BoxLastRect(bool isActivated = true) {
            var lastRect = GUILayoutUtility.GetLastRect();
            BoxRect(lastRect, isActivated);
        }
        private static void BoxRect(Rect targetRect, bool isActivated = true, bool withTop = true) {
            var leftBar = new Rect(targetRect) {
                width = 3
            };

            var rightBar = new Rect(targetRect);
            rightBar.x += rightBar.width - 3;
            rightBar.width = 3;

            var topBar = new Rect(targetRect) {
                height = 3
            };
            topBar.x += 3;
            topBar.width -= 6;

            var bottomBar = new Rect(targetRect);
            bottomBar.y += bottomBar.height - 3;
            bottomBar.height = 3;
            bottomBar.x += 3;
            bottomBar.width -= 6;

            DrawOutlineBox(leftBar, isActivated);
            DrawOutlineBox(rightBar, isActivated);
            DrawOutlineBox(bottomBar, isActivated);
            if (withTop) {
                DrawOutlineBox(topBar, isActivated);
            }

            DrawBox(leftBar, isActivated);
            DrawBox(rightBar, isActivated);
            DrawBox(bottomBar, isActivated);
            if (withTop) {
                DrawBox(topBar, isActivated);
            }
        }
        private static void DrawBox(Rect rectPosition, bool isActivated = true) {
            Color boxColor = default;

            if (EditorGUIUtility.isProSkin) {
                if (isActivated) {
                    boxColor = new Color(0.66f, 0.66f, 0.66f, 1);
                } else {
                    boxColor = new Color(0.38f, 0.38f, 0.38f, 1);
                }
            } else {
                if (isActivated) {
                    boxColor = new Color(0.55f, 0.55f, 0.55f, 1);
                } else {
                    boxColor = new Color(0.67f, 0.67f, 0.67f, 1);
                }
            }

            DrawBox(rectPosition, boxColor);
        }
        private static void DrawBox(Rect rectPosition, Color boxColor) {
            GUI.backgroundColor = boxColor;
            GUI.Box(rectPosition, "", whiteBoxStyle);
            GUI.backgroundColor = Color.white;
        }
        private static void DrawOutlineBox(Rect rectPosition, bool isActivated = true) {
            // Adjust Position
            rectPosition.width += 2;
            rectPosition.height += 2;

            // Draw Box
            if (EditorGUIUtility.isProSkin) {
                if (isActivated) {
                    DrawBox(rectPosition, new Color(0.24f, 0.24f, 0.24f, 1));
                } else {
                    DrawBox(rectPosition, new Color(0.21f, 0.21f, 0.21f, 1));
                }
            } else {
                if (isActivated) {
                    DrawBox(rectPosition, new Color(0.35f, 0.35f, 0.35f, 1));
                } else {
                    DrawBox(rectPosition, new Color(0.5f, 0.5f, 0.5f, 1));
                }
            }
        }
        #endregion
    }
}

#endif
