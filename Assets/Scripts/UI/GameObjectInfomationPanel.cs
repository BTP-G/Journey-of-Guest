using EditorAttributes;
using TMPro;
using UnityEngine;

namespace JoG.UI {

    public class GameObjectInfomationPanel : MonoBehaviour {
        [field: SerializeField, Required] public TMP_Text NameText { get; private set; }
        [field: SerializeField, Required] public TMP_Text DescriptionText { get; private set; }

        public void SetTip(string name, string description) {
            NameText.text = name;
            DescriptionText.text = description;
        }

        public void SetTipName(string name) => NameText.text = name;

        public void SetTipDescription(string description) => DescriptionText.text = description;

        public void UpdateView(GameObject gameObject) {
            var tipName = string.Empty;
            var tipDescription = string.Empty;
            if (gameObject != null) {
                if (gameObject.TryGetComponent<OutlineController>(out var outlineController)) {
                    outlineController.ShowOutline(0.1f);
                }
                if (gameObject.TryGetComponent<IInfomationProvider>(out var infomationProvider)) {
                    tipName = infomationProvider.Name;
                    tipDescription = infomationProvider.Description;
                }
            }
            SetTip(tipName, tipDescription);
        }
    }
}