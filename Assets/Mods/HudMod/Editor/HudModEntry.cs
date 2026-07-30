using Cysharp.Threading.Tasks;
using UnityEngine;

namespace JoG.Modding {

    /// <summary>
    /// Mod entry point. Loaded by the ModManager when the player enables this mod in the mod list.
    /// Lives in HudMod.dll (separate from the main JoG assembly).
    /// Creates/destroys the HudMod MonoBehaviour as a DontDestroyOnLoad GameObject.
    /// </summary>
    public class HudModEntry : Mod {
        private GameObject _hudGameObject;

        protected override UniTask OnEnableAsync() {
            CreateHud();
            return UniTask.CompletedTask;
        }

        protected override UniTask OnDisableAsync() {
            DestroyHud();
            return UniTask.CompletedTask;
        }

        private void CreateHud() {
            if (_hudGameObject != null) return;
            _hudGameObject = new GameObject("HudMod");
            Object.DontDestroyOnLoad(_hudGameObject);
            _hudGameObject.AddComponent<HudMod>();
        }

        private void DestroyHud() {
            if (_hudGameObject == null) return;
            Object.Destroy(_hudGameObject);
            _hudGameObject = null;
        }
    }
}
