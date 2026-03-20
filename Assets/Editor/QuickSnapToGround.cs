using UnityEngine;
using UnityEditor;
using JoG;

public static class QuickSnapToGround {

    [MenuItem("Tools/Snap Selected To Ground _F10")] // 快捷键 F10
    public static void SnapToGround() {
        foreach (GameObject @object in Selection.gameObjects) {
            var origin = @object.transform.position;
            // 从物体位置向下发射射线
            if (Physics.Raycast(origin + new Vector3(0, 100f, 0), Vector3.down, out var hit, 1000f, LayerMasks.Default, QueryTriggerInteraction.Ignore)) {
                // 记录撤销操作
                Undo.RecordObject(@object.transform, "Snap to Ground");
                // 移动物体，保持原有的相对高度（如果需要贴地，offset设为0）
                @object.transform.position = hit.point;
                // 如果你想让物体底部贴地，可能需要减去物体自身高度的一半
                Debug.DrawLine(origin, hit.point, Color.red, 5);
                Debug.Log(hit.point);
                Debug.Log(hit.distance);
            }
        }
    }
}