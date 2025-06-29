using UnityEngine;

namespace JoG.Magic {

    public class AverageManaZone : ManaZone {
        [SerializeField] protected uint unitManaQuantity;

        public override uint GetUnitManaQuantity() {
            return unitManaQuantity;
        }
    }
}