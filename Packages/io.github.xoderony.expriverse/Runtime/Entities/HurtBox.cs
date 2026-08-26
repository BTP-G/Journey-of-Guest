using UnityEngine;
using VContainer;

namespace Expriverse {

    [RequireComponent(typeof(Collider))]
    public sealed class HurtBox : MonoBehaviour, IComponent {
        [Inject] public Entity Entity { get; internal set; }

        object IComponent.Key => gameObject.name;
    }
}
