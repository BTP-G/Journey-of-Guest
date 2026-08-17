//RealToon Shader Cache
//MJQStudioWorks
//?025

using System.IO;
using UnityEditor;
using UnityEngine;

namespace RealToon.Tools {
    public class RealToonShaderCache : EditorWindow {

        #region Variables

        private static float WinHig = 300;
        private static EditorWindow EdiWin;
        private static int countsha = 0;

        private static string shaderCachePath = Directory.GetParent(Application.dataPath).ToString() + "/Library/ShaderCache/shader";

        private static string infospace = null;

        private static string[] rt_shaders =
                                { "D_Default"
                                , "D_Fade_Transparency"
                                , "D_Refraction"
                                , "L_Default"
                                , "L_Fade_Transparency"
                                , "T_Default"
                                , "T_Fade_Transparency"
                                , "T_Refraction"
                                , "D_Default_URP"
                                , "D_Default_HDRP"
                                , "RealToon_Sobel_Outline_FX"
                                , "DeNorSobOutline"};

        #endregion

        [MenuItem("Window/RealToon/RealToon Shader Cache")]
        private static void Init() {
            EdiWin = GetWindow<RealToonShaderCache>(true);
            EdiWin.titleContent = new GUIContent("RealToon Shader Cache");
            WinHig = 200;
            EdiWin.minSize = new Vector2(440, WinHig);
            EdiWin.maxSize = new Vector2(440, WinHig);

            CountRTCache();
            infospace = "There are " + countsha + " RealToon Shaders & Effects cached.";

        }

        private void OnGUI() {

            var lblcenstyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };

            GUILayout.Space(10);
            EditorGUILayout.BeginVertical();

            GUILayout.Label(infospace, lblcenstyle);

            CountRTCache();
            infospace = "There are " + countsha + " RealToon Shaders cached.";

            GUILayout.Space(10);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            if (GUILayout.Button("Reduce/Clean Cached RealToon Shader")) {

                foreach (var rt_sha in rt_shaders) {
                    var direc = Directory.GetDirectories(@shaderCachePath, rt_sha + "*");

                    foreach (var dir in direc) {
                        if (direc.Length == 1) {
                            Debug.Log("RealToon Shader Cache: " + rt_sha + " Shader Cache removed.");
                            Directory.Delete(dir.ToString(), true);
                        }
                    }

                    if (direc.Length == 0) {
                        Debug.LogWarning("RealToon Shader Cache: " + rt_sha + " Shader already removed or not present.");
                    }
                }
            }

            if (GUILayout.Button("Re-import RealToon Shader")) {
                AssetDatabase.ImportAsset("Assets/RealToon", ImportAssetOptions.ImportRecursive);
            }

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            GUILayout.Label("Note:");
            GUILayout.Label("*This will reduce or clear cached RealToon Shaders.\n" +
                "*Useful for after update RealToon Shader or Re-import RealToon Shader.\n" +
                "*'Re-import RealToon Shader', will cache RealToon Shaders again.");

            EditorGUILayout.EndVertical();

        }

        private static void CountRTCache() {
            countsha = 0;
            foreach (var rt_sha in rt_shaders) {
                var direc = Directory.GetDirectories(@shaderCachePath, rt_sha + "*");

                foreach (var dir in direc) {
                    ++countsha;
                }
            }
        }
    }
}
