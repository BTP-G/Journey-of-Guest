using EditorAttributes;
using TMPro;
using Unity.Netcode;
using UnityEngine.UI;

namespace Expriverse.Gameplay {

    public class ObjectiveController : NetworkBehaviour {

        [Required] public TMP_Text labelText;

        [Required] public Toggle2 isCompleteToggle;

        public bool IsComplete {
            get => isCompleteToggle.isOn;
            set => isCompleteToggle.isOn = value;
        }

        protected override void OnSynchronize<T>(ref BufferSerializer<T> serializer) {
            var isComplete = isCompleteToggle.isOn;
            serializer.SerializeValue(ref isComplete);
            isCompleteToggle.isOn = isComplete;
        }

        protected virtual void Awake() {
            isCompleteToggle.onValueChanged.AddListener(OnToggleChanged);
        }

        [Rpc(SendTo.NotMe, InvokePermission = RpcInvokePermission.Owner)]
        private void SetIsCompleteRpc(bool isComplete) {
            isCompleteToggle.isOn = isComplete;
        }

        private void OnToggleChanged(bool arg0) {
            if (IsOwner) {
                SetIsCompleteRpc(arg0);
            }
        }
    }
}
