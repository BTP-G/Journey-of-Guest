using Cysharp.Text;
using EditorAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JoG.UI {

    public class BuffIcon : MonoBehaviour {
        private int countCache;
        [field: SerializeField, Required] public Image IconImage { get; private set; }

        [field: SerializeField, Required] public TMP_Text CountText { get; private set; }

        public void UpdateView(Sprite icon, int count) {
            IconImage.sprite = icon;
            if (countCache == count) {
                return;
            }

            using var sb = ZString.CreateStringBuilder(true);
            CountText.SetText(sb);
            countCache = count;
        }

        protected void Reset() {
            IconImage = GetComponentInChildren<Image>();
            CountText = GetComponentInChildren<TMP_Text>();
        }
    }
}
