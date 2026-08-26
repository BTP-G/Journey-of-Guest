using UnityEngine;
using UnityEngine.UI;

namespace Expriverse {

    [DisallowMultipleComponent]
    internal sealed class ApplicationInitializationView : MonoBehaviour {
        [SerializeField] private Text _statusText;

        public void SetStatus(string status) {
            gameObject.SetActive(true);
            _statusText.text = status;
        }

        public void Close() {
            Destroy(gameObject);
        }
    }
}
