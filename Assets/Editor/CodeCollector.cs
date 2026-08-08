using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;

public class CodeCollector : EditorWindow {
    [MenuItem("Tools/Collect Selected C# Scripts for Appendix")]
    private static void CollectSelectedScripts() {
        // 获取 Project 窗口中选中的对象
        var selectedObjects = Selection.objects;
        if (selectedObjects == null || selectedObjects.Length == 0) {
            EditorUtility.DisplayDialog("提示", "请先在 Project 窗口中选中一个或多个 .cs 文件，再运行此工具。", "确定");
            return;
        }

        // 获取所有选中资源的路径，并过滤出 .cs 文件
        var csFilePaths = new List<string>();
        foreach (var obj in selectedObjects) {
            var path = AssetDatabase.GetAssetPath(obj);
            if (!string.IsNullOrEmpty(path) && path.EndsWith(".cs")) {
                csFilePaths.Add(path);
            }
        }

        if (csFilePaths.Count == 0) {
            EditorUtility.DisplayDialog("提示", "所选对象中没有 C# 脚本文件（.cs）。\n请确保选中的是脚本文件，而不是文件夹或其他资源。", "确定");
            return;
        }

        // 让用户选择输出 txt 文件路径
        var outputPath = EditorUtility.SaveFilePanel("保存 txt 文件", "", "Appendix_Scripts", "txt");
        if (string.IsNullOrEmpty(outputPath)) {
            return;
        }

        var sb = new StringBuilder();
        var collectedCount = 0;
        var skippedCount = 0;
        var skippedFiles = new List<string>();

        foreach (var filePath in csFilePaths) {
            // 跳过全注释文件
            if (IsFileFullyCommented(filePath)) {
                skippedCount++;
                skippedFiles.Add(Path.GetFileName(filePath));
                continue;
            }

            collectedCount++;
            var fileName = Path.GetFileName(filePath);
            //sb.AppendLine($"// ===== 文件：{fileName} =====");

            var content = ReadFileWithAutoEncoding(filePath);
            // 去空行
            var lines = content.Split(new[] { "\r\n", "\n" }, System.StringSplitOptions.None);
            foreach (var line in lines) {
                if (!string.IsNullOrWhiteSpace(line)) {
                    sb.AppendLine(line);
                }
            }
        }

        if (collectedCount == 0) {
            EditorUtility.DisplayDialog("提示", "所有选中的文件都是全注释废弃文件，没有可收集的有效代码。", "确定");
            return;
        }

        File.WriteAllText(outputPath, sb.ToString(), new UTF8Encoding(true));

        // 完成提示
        var msg = $"✅ 选中了 {csFilePaths.Count} 个脚本\n"
                   + $"📄 已收集 {collectedCount} 个有效文件（已去空行）\n"
                   + $"🚫 跳过 {skippedCount} 个全注释废弃文件\n\n"
                   + $"📁 保存位置：{outputPath}\n\n"
                   + "📌 下一步（Word 中生成行号）：\n"
                   + "1. 打开该 txt 文件，全选复制\n"
                   + "2. 粘贴到 Word 附录中\n"
                   + "3. 全选代码 → 开始菜单 → 编号 → 选择「1. 2. 3.」格式\n"
                   + "4. 行号自动生成，符合规范";

        if (skippedFiles.Count > 0) {
            msg += "\n\n跳过的文件：\n" + string.Join("\n", skippedFiles);
        }

        var openFolder = EditorUtility.DisplayDialog("完成", msg, "打开文件夹", "关闭");
        if (openFolder) {
            EditorUtility.RevealInFinder(outputPath);
        }
    }

    /// <summary> 判断文件是否所有非空行都被 // 注释 </summary>
    private static bool IsFileFullyCommented(string filePath) {
        try {
            var lines = File.ReadAllLines(filePath, Encoding.UTF8);
            foreach (var line in lines) {
                var trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed)) {
                    continue;
                }

                if (!trimmed.StartsWith("//")) {
                    return false;
                }
            }
            return true;
        } catch { return false; }
    }

    /// <summary> 自动检测编码并读取文本 </summary>
    private static string ReadFileWithAutoEncoding(string filePath) {
        var raw = File.ReadAllBytes(filePath);
        if (raw.Length >= 3 && raw[0] == 0xEF && raw[1] == 0xBB && raw[2] == 0xBF) {
            return Encoding.UTF8.GetString(raw, 3, raw.Length - 3);
        }

        try {
            var utf8Str = Encoding.UTF8.GetString(raw);
            if (!utf8Str.Contains('\uFFFD')) {
                return utf8Str;
            }
        } catch { }

        try {
            var gb2312 = Encoding.GetEncoding("GB2312");
            return gb2312.GetString(raw);
        } catch {
            return Encoding.Default.GetString(raw);
        }
    }
}