using UnityEngine;
using UnityEngine.UI;

namespace Expriverse.UI.Menu {

    public class MultiplayerPanel : MenuPanel {
        [SerializeField] private Button _closeButton;

        public override void Initialize(MenuManager manager) {
            _closeButton.onClick.AddListener(() => manager.ClosePanel(this));
        }
    }
}
