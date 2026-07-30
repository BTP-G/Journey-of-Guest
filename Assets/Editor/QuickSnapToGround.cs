using UnityEngine;
using UnityEditor;
using JoG;

public static class QuickSnapToGround {

    [MenuItem("Tools/Snap Selected To Ground _F10")] // 快捷键 F10
    public static void SnapToGround() {
        foreach (GameObject @object in Selection.gameObjects) {
            var origin = @object.transform.position;
            if (Physics.Raycast(origin + new Vector3(0, 100f, 0), Vector3.down, out var hit, 1000f, LayerMasks.Default, QueryTriggerInteraction.Ignore)) {
                Undo.RecordObject(@object.transform, "Snap to Ground");
                @object.transform.position = hit.point;
                Debug.DrawLine(origin, hit.point, Color.red, 5);
            }
        }
    }
}