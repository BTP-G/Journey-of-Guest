using Expriverse.UI.Popup;
using UnityEngine;
using VContainer;

namespace Expriverse.Modding {

    public class ModListController : MonoBehaviour {
        [Inject] internal IModManager _modManager;
        [Inject] internal LoaderPopup _loaderPopup;
        private Transform modListContent;
        [SerializeField] private ModListItem modItemTemplate;

        private void Awake() {
            modListContent = modItemTemplate.transform.parent;
        }

        private void Start() {
            foreach (var mod in _modManager.ModSpan) {
                var item = Instantiate(modItemTemplate, modListContent);

                item.nameText.text = mod.Name;
                item.authorText.text = mod.Author;
                item.descriptionText.text = mod.Description;
                item.versionText.text = mod.Version.ToString();
                item.enabledToggle.SetIsOnWithoutNotify(mod.Enabled);
                item.enabledToggle.onValueChanged.AddListener(async isOn => {
                    using (_loaderPopup.Show()) {
                        if (isOn) {
                            await _modManager.EnableModAsync(mod.Id);
                        } else {
                            await _modManager.DisableModAsync(mod.Id);
                        }
                    }
                });
                item.gameObject.SetActive(true);
            }
        }
    }
}
