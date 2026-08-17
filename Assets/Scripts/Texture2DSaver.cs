using EditorAttributes;
using UnityEngine;

namespace JoG {

    public class JoGTest : MonoBehaviour {
        public RenderTexture renderTexture;
        public Camera renderCamera;
        public GameObject targetObject;

        [Button]
        public void Save() {
            renderCamera.targetTexture = renderTexture;
            renderCamera.Render();
            RenderTexture.active = renderTexture;
            // 将 RenderTexture 转为 Texture2D
            var tex = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);
            tex.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);

            tex.Apply();

            // 保存为 PNG（可选）
            var bytes = tex.EncodeToPNG();
            System.IO.File.WriteAllBytes(Application.dataPath + "/AssetsPackage/Textures/" + targetObject.name + ".png", bytes);

            // 转换为 Sprite（用于 UI）
            var iconSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }
    }
}
