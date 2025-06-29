using EditorAttributes;
using JoG.BuffSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JoG.UI.Buff {

    public class BuffIcon : MonoBehaviour {
        [field: SerializeField, Required] public Image IconImage { get; private set; }

        [field: SerializeField, Required] public TMP_Text CountText { get; private set; }

        public void UpdateView(IBuff buff) {
            IconImage.sprite = buff.IconSprite;
            CountText.text = buff.Count.ToString();
        }

        public void UpdateView(Sprite icon, ushort count) {
            IconImage.sprite = icon;
            CountText.text = count.ToString();
        }

        public void UpdateView(ushort count) {
            CountText.text = count.ToString();
        }

        protected void Reset() {
            IconImage = GetComponent<Image>();
            CountText = GetComponentInChildren<TMP_Text>();
        }
    }
}