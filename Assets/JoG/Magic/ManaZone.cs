using System;
using UnityEngine;

namespace JoG.Magic {

    [RequireComponent(typeof(Collider))]
    public abstract class ManaZone : MonoBehaviour {

        public static uint GetAvarageManaQuantity(ReadOnlySpan<ManaZone> magicZones) {
            if (magicZones.Length == 0) return 0u;
            var sum = 0u;
            for (var i = 0; i < magicZones.Length; ++i) {
                sum += magicZones[i].GetUnitManaQuantity();
            }
            return (uint)(sum / magicZones.Length);
        }

        public abstract uint GetUnitManaQuantity();
    }
}