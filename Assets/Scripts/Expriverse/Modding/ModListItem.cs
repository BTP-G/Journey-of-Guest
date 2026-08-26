using EditorAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Expriverse.Modding {

    public class ModListItem : MonoBehaviour {
        [Required] public TMP_Text nameText;
        [Required] public TMP_Text authorText;
        [Required] public TMP_Text descriptionText;
        [Required] public TMP_Text versionText;
        [Required] public Toggle enabledToggle;
    }
}
