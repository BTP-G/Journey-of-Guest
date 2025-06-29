using UnityEngine;

namespace JoG.HitColliders {

    [RequireComponent(typeof(Collider))]
    [DisallowMultipleComponent]
    public class CharacterCollider : MonoBehaviour, IDamageable, IHealable {
        public float damageCofficient;
        public float healingCofficient;
        public IHealth Health { get; private set; }

        public void Handle(HealingMessage message) {
            message.cofficient += damageCofficient;
            Health.Handle(message);
        }

        public void Handle(DamageMessage message) {
            message.cofficient += healingCofficient;
            Health.Handle(message);
        }

        protected void Awake() {
            Health = GetComponentInParent<IHealth>();
        }
    }
}