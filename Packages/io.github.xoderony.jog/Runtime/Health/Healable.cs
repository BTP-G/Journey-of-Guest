using UnityEngine;
using VContainer;

namespace JoG.Health {

    [RequireComponent(typeof(Collider))]
    public class Healable : MonoBehaviour, IComponent, IHealable {

        [Inject]
        internal Entity entity;

        [Inject]
        internal HealthChangeRouter router;

        public Entity Entity => entity;

        object IComponent.Key => gameObject.name;

        public bool CanTakeHeal(Entity attacker) {
            return router.CanHeal(attacker, entity);
        }

        public void TakeHeal(ref HealthChangeMessage message, Entity attacker) {
            router.Broadcast(attacker, entity, ref message);
        }
    }
}
