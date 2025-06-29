using EditorAttributes;
using GuestUnion;
using JoG.Character;
using JoG.InteractionSystem;
using JoG.InventorySystem;
using JoG.Projectiles;
using JoG.Weapons.Datas;
using JoG.Weapons.Magazines;
using System;
using Unity.Netcode;
using UnityEngine;

namespace JoG.Weapons {

    public enum FireMode {
        FullyAutomatic,
        SemiAutomatic,
    }

    public class Gun : NetworkBehaviour, IGun, IInteractable, IItem {
        public const float MAX_PASSED_TIME = 0.3f;
        public FireMode fireMode;
        [DataTable(showLabels: false)] public Magazine magazine = new(30);

        private readonly NetworkVariable<NetworkBehaviourReference> ownerBody = new();

        [SerializeField] private AudioSource audioSource;
        [SerializeField] private ParticleSystem muzzleFlash;
        private float nextFireTime;
        private bool isPressed;
        [field: SerializeField] public GunData Data { get; private set; }
        [field: SerializeField] public Transform BulletSpawnPoint { get; private set; }
        [field: SerializeField] public Transform MagazineTransform { get; private set; }
        public ProjectileData BulletPrefab { get; private set; }

        /// <summary>SyncType. Write: ServerOnly</summary>
        public CharacterBody OwnerBody {
            get {
                ownerBody.Value.TryGet<CharacterBody>(out var character, NetworkManager);
                return character;
            }

            set => ownerBody.Value = value;
        }

        public ItemData ItemData => throw new NotImplementedException();

        public Vector2 GetRandomSpread() => (1 - Mathf.Min(1, (Time.time - nextFireTime) / Data.timeOfStabilizing)) * Data.RandomSpread;

        public Vector2 GetMaxSpread() => (1 - Mathf.Min(1, (Time.time - nextFireTime) / Data.timeOfStabilizing)) * Data.spread;

        public void PreformInteraction(Interactor interactor) {
        }

        public Interactability GetInteractability(Interactor interactor) {
            return OwnerBody is null ? Interactability.Available : Interactability.Disabled;
        }

        public void PushTriggerState(bool down) {
            if (down) {
                if (fireMode is FireMode.FullyAutomatic && CanFire()) {
                    FireAuthority();
                } else if (fireMode is FireMode.SemiAutomatic) {
                    if (!isPressed && CanFire()) {
                        FireAuthority();
                    }
                }
            }
            isPressed = down;
        }

        public bool CanFire() => Time.time > nextFireTime && magazine.count > 0;

        public void OnAdded(GameObject owner) {
            throw new NotImplementedException();
        }

        public void OnRemoved(GameObject owner) {
            throw new NotImplementedException();
        }

      

        protected virtual void FireAuthority() {
            BulletSpawnPoint.GetPositionAndRotation(out var position, out var rotation);
            var spread = GetRandomSpread();
            var no = Instantiate(BulletPrefab.GetComponent<NetworkObject>(), position, rotation.ApplySpread(spread.y, spread.x));
            no.GetComponent<ProjectileData>().ownerReference = OwnerBody.NetworkObject;
            no.SpawnWithOwnership(OwnerClientId);
            audioSource.Play();
            muzzleFlash.Play();
            nextFireTime = Time.time + Data.fireInterval;
            --magazine.count;
        }
    }
}