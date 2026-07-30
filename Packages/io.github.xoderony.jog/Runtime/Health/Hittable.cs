using UnityEngine;
using VContainer;

namespace JoG.Health {

    [RequireComponent(typeof(Collider))]
    public class Hittable : MonoBehaviour, IHittable, IComponent {

        [Inject]
        internal Entity entity;

        [Inject]
        internal HitRouter router;

        public Entity Entity => entity;

        object IComponent.Key => gameObject.name;

        public void TakeHit(in HitMessage message, Entity source) {
            router.Broadcast(source, entity, message);
        }
    }
}
