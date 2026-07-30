using System.Linq;
using UnityEngine;

namespace JoG.UI {

    [DisallowMultipleComponent]
    public class TabGroup : MonoBehaviour {
        public TabToggle[] tabs;

        internal void SwitchTo(TabToggle tab) {
            foreach (var t in tabs) {
                t.Set(t == tab);
            }
        }

        private void Reset() {
            tabs = GetComponentsInChildren<TabToggle>(true)
                .Where(t => t.transform.parent == transform)
                .ToArray();
        }
    }
}
