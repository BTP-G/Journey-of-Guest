using System.Collections.Generic;
using UnityEngine;
using Xoderony.ObjectPool.Generic;

namespace Expriverse.UI.Menu {

    public class MenuManager : MonoBehaviour {
        private readonly Stack<MenuPanel> _activePanels = new();

        public void OpenPanel(MenuPanel panel) {
            if (_activePanels.TryPeek(out var current)) {
                if (current == panel) {
                    return;
                }

                current.gameObject.SetActive(false);
                current.InvokeOnClose();
            }
            panel.gameObject.SetActive(true);
            panel.InvokeOnOpen();
            _activePanels.Push(panel);
        }

        public void ClosePanel(MenuPanel panel) {
            if (_activePanels.TryPeek(out var current) && current == panel) {
                current.gameObject.SetActive(false);
                current.InvokeOnClose();
                _activePanels.Pop();
                if (_activePanels.TryPeek(out var prev)) {
                    prev.gameObject.SetActive(true);
                    prev.InvokeOnOpen();
                }
            }
        }

        private void Awake() {
            using (ListPool<MenuPanel>.Rent(out var buffer)) {
                GetComponentsInChildren(true, buffer);
                foreach (var panel in buffer) {
                    panel.gameObject.SetActive(false);
                    panel.Initialize(this);
                }
            }
        }
    }
}
