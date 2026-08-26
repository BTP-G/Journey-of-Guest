using UnityEngine;
using VContainer;

namespace Expriverse.Health {

    [RequireComponent(typeof(Collider))]
    public class Damageable : MonoBehaviour, IComponent, IDamageable {

        [Inject]
        internal Entity entity;

        [Inject]
        internal HealthChangeRouter router;

        public Entity Entity => entity;

        object IComponent.Key => gameObject.name;

        public bool CanTakeDamage(Entity attacker) {
            return router.CanDamage(attacker, entity);
        }

        public void TakeDamage(ref HealthChangeMessage message, Entity attacker) {
            router.Broadcast(attacker, entity, ref message);
        }
    }
}
