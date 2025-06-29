using TMPro;
using UnityEngine;

namespace JoG.UI {

    public class TextCopyExtend : MonoBehaviour {
        [SerializeField] private TMP_Text _text;

        public void CopyTextToClipboard() {
            GUIUtility.systemCopyBuffer = _text.text;
        }
    }
}