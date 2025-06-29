using Cysharp.Threading.Tasks;
using EditorAttributes;
using JoG.DebugExtensions;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace JoG.UI {

    public class LoadingPanelManager : MonoBehaviour {
        private static LoadingPanelManager _instance;

        [SerializeField, Required] private GameObject _panel;
        [SerializeField, Required] private Transform _loadingImageTransform;
        [SerializeField, Required] private TMP_Text _loadingMessageText;
        [SerializeField, Required] private TMP_Text _loadingTimeText;

        private float _loadingTime;

        public static async UniTask<T> Loading<T>(UniTask<T> task,string message = "加载中······") {
            if (_instance) {
                _instance.Show(message);
            }
            var result = await task;
            if (_instance) {
                _instance.Hide();
            }
            return result;
        }

        public static async UniTask Loading(UniTask task, string message = "加载中······") {
            if (_instance) {
                _instance.Show(message);
            }
            await task;
            if (_instance) {
                _instance.Hide();
            }
        }

        public static async UniTask Loading(Task task, string message = "加载中······") {
            if (_instance) {
                _instance.Show(message);
            }
            await task;
            if (_instance) {
                _instance.Hide();
            }
        }

        public void Show(string message) {
            _panel.SetActive(true);
            _loadingMessageText.SetText(message);
            enabled = true;
        }

        public void Hide() {
            _panel.SetActive(false);
            enabled = false;
        }

        private void Awake() {
            if (_instance) {
                this.LogWarning("已经存在一个加载界面的实例了！");
                Destroy(gameObject);
                return;
            }
            _instance = this;
            Hide();
        }

        private void OnEnable() {
            _loadingTime = 0;
        }

        private void Update() {
            var deltaTime = Time.deltaTime;
            _loadingImageTransform.Rotate(0, 0, 360 * deltaTime);
            _loadingTime += deltaTime;
            _loadingTimeText.SetText(((int)_loadingTime).ToString());
        }

        private void OnDestroy() {
            if (_instance == this) {
                _instance = null;
            }
        }
    }
}