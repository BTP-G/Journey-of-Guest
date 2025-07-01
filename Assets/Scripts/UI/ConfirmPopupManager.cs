using Cysharp.Threading.Tasks;
using DG.Tweening;
using EditorAttributes;
using JoG.DebugExtensions;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JoG.UI {

    public class ConfirmPopupManager : MonoBehaviour {
        private static ConfirmPopupManager _instance;
        [SerializeField, Required] private GameObject _popup;
        [SerializeField, Required] private Graphic _backgroundGraphic;
        [SerializeField, Required] private Transform _messagePanel;
        private Action _confirmAction;
        private Action _cancelAction;

        [field: SerializeField, Required] public TMP_Text MessageText { get; private set; }

        [field: SerializeField, Required] public Button ConfirmButton { get; private set; }

        [field: SerializeField, Required] public Button CancelButton { get; private set; }

        public static void Popup(string message = "是否确认？", Action confirmAction = null, Action cancelAction = null) {
            if (_instance == null) {
                Debug.LogError("未找到弹窗实例，弹出失败！");
                return;
            }
            _instance.Show(message, confirmAction, cancelAction);
        }

        public static UniTask<bool> PopupAsync(string message = "是否确认？") {
            if (_instance == null) {
                Debug.LogError("未找到弹窗实例，弹出失败！");
                return UniTask.FromResult(false);
            }
            var tcs = new UniTaskCompletionSource<bool>();
            Popup(message, () => tcs.TrySetResult(true), () => tcs.TrySetResult(false));
            return tcs.Task;
        }

        public void Show(string message = "是否确认？", Action confirmAction = null, Action cancelAction = null) {
            MessageText.text = message;
            _confirmAction = confirmAction;
            _cancelAction = cancelAction;
            _backgroundGraphic.CrossFadeAlpha(0, 0, false);
            _backgroundGraphic.CrossFadeAlpha(1f, 0.3f, false);
            _messagePanel.localScale = Vector3.zero;
            _messagePanel.DOScale(Vector3.one, 0.3f);
            _popup.SetActive(true);
        }

        public void Hide() => _popup.SetActive(false);

        private void Awake() {
            if (_instance) {
                this.LogWarning("已经存在一个确认弹窗的实例！");
                Destroy(gameObject);
                return;
            }
            _instance = this;
            ConfirmButton.onClick.AddListener(() => {
                _confirmAction?.Invoke();
                Hide();
            });
            CancelButton.onClick.AddListener(() => {
                _cancelAction?.Invoke();
                Hide();
            });
            Hide();
        }

        private void OnDestroy() {
            if (_instance == this) {
                _instance = null;
            }
        }
    }
}