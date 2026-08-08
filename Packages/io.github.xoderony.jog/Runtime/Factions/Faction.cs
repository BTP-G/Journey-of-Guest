using System;
using UnityEngine;

namespace JoG {

    [Serializable]
    public sealed class Faction : IComponent {

        [field: SerializeField]
        public int Id { get; private set; }

        public Faction() { }

        public Faction(int id) {
            Id = id;
        }

        public bool IsAlliedWith(Faction other) {
            return other != null && Id == other.Id;
        }

        public bool IsHostileTo(Faction other) {
            return !IsAlliedWith(other);
        }
    }
}
