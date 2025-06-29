using JoG.Character;
using UnityEngine;

namespace JoG.Magic {

    public abstract class Spell : ScriptableObject {
        public uint manaCost;
        public AudioClip spellClip;

        public abstract void Cast(CharacterBody caster, in Vector3 position, in Quaternion rotation);
    }
}