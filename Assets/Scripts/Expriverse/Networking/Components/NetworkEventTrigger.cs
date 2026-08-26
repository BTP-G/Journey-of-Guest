using Unity.Netcode;
using UnityEngine.Events;

namespace Expriverse.Networking.Components {

    public class NetworkEventTrigger : NetworkBehaviour {
        public UnityEvent2 onTrigger = new();

        [Rpc(SendTo.Everyone)]
        public void InvokeRpc() {
            onTrigger.Invoke();
        }
    }
}
