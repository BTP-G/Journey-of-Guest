using EditorAttributes;
using GuestUnion;
using JoG.Character;
using JoG.InteractionSystem;
using System;
using Unity.Netcode;
using UnityEngine;
using URandom = UnityEngine.Random;

namespace JoG.Magic {

    public class Wand : NetworkBehaviour, IInteractable, IInfomationProvider {
        public Spell spell;
        public uint manaCapacity = 100;
        [NonSerialized] public float value;
        [Range(1, float.MaxValue)] public float chargingSpeed;
        [Clamp(0.02f, 50f)] public float cooldown = 1f;
        public Vector2 maxSpread;
        private float _lastCastTime;
        private NetworkVariable<NetworkObjectReference> _ownerReference = new();

        public NetworkObject Owner {
            set => _ownerReference.Value = value;
        }

        [field: SerializeField] public Transform SpawnPoint { get; private set; }
        [field: SerializeField] public AudioSource AudioSource { get; private set; }
        public Vector2 CurrentRandomSpread => URandom.insideUnitCircle * maxSpread;

        public virtual void Cast(CharacterBody caster) {
            if (spell.manaCost > value || (Time.time - _lastCastTime) < cooldown) return;
            value -= spell.manaCost;
            SpawnPoint.GetPositionAndRotation(out var position, out var rotation);
            var spread = CurrentRandomSpread;
            spell.Cast(caster, position, rotation.ApplySpread(spread.y, spread.x));
            if (spell.spellClip != null) {
                AudioSource.PlayOneShot(spell.spellClip);
            }
            _lastCastTime = Time.time;
        }

        string IInfomationProvider.GetString(string token) {
            if (token is "name") {
                return "火球杖";
            }
            if (token is "description") {
                return "发射火球";
            }
            return null;
        }

        Interactability IInteractable.GetInteractability(Interactor interactor) {
            return _ownerReference.Value.TryGet(out _) ? Interactability.Disabled : Interactability.Available;
        }

        void IInteractable.PreformInteraction(Interactor interactor) {
        }

        private void Update() {
            if (value < manaCapacity) {
                value += chargingSpeed * Time.deltaTime;
                if (value > manaCapacity) {
                    value = manaCapacity;
                }
            }
        }
    }
}