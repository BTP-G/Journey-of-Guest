using UnityEditor;
using UnityEngine;

namespace EditorAttributes.Editor {
    [CustomPropertyDrawer(typeof(ClampAttribute))]
    public class ClampDrawer : MinMaxAxisDrawer {
        protected override void MinMaxAxis((float, float) minMaxX, (float, float) minMaxY, (float, float) minMaxZ, (float, float) minMaxW, ref Vector4 vector) {
            var x = Mathf.Clamp(vector.x, minMaxX.Item1, minMaxX.Item2);
            var y = Mathf.Clamp(vector.y, minMaxY.Item1, minMaxY.Item2);
            var z = Mathf.Clamp(vector.z, minMaxZ.Item1, minMaxZ.Item2);
            var w = Mathf.Clamp(vector.w, minMaxW.Item1, minMaxW.Item2);

            vector = new Vector4(x, y, z, w);
        }
    }
}
