using EditorAttributes;
using JoG.UnityObjectExtensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JoG.Lobby.View {

    public class LobbyListView : MonoBehaviour {
        [field: SerializeField, Required] public Transform Content { get; private set; }
        [field: SerializeField, Required] public Button LobbyListButton { get; private set; }
        [field: SerializeField, Required] public Button ReturnButton { get; private set; }
        [field: SerializeField, Required] public Button SearchButton { get; private set; }
        [field: SerializeField, Required] public TMP_InputField LobbyNameFilterInput { get; private set; }

        public void Clear() {
            foreach (Transform t in Content) {
                t.gameObject.Destroy(0.5f);
            }
        }

        protected void Awake() {
            LobbyListButton.onClick.AddListener(() => gameObject.SetActive(true));
            ReturnButton.onClick.AddListener(() => gameObject.SetActive(false));
        }
    }
}