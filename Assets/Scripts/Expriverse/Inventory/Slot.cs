using EditorAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Expriverse.Inventory {

    public partial class Slot : MonoBehaviour {
        [Required] public Image iconImage;
        [Required] public TextMeshProUGUI countText;
    }
}
