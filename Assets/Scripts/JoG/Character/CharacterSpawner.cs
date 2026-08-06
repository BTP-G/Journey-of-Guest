using JoG.Networking;
using System;
using Unity.Netcode;
using UnityEngine;
using VContainer;

namespace JoG.Character {

    public class CharacterSpawner : NetworkBehaviour {

        private static readonly NetworkObjectReference NullBodyReference = new((NetworkObject)null);

        [Inject] internal NetworkObjectFactory networkObjectFactory;

        private readonly NetworkVariable<NetworkObjectReference> _bodyReference = new(
            NullBodyReference,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner
        );

        private CharacterInputBinding _inputBinding;

        private bool _isBodyReferencePending;

        public event Action<CharacterEntity, CharacterEntity> BodyChanged;

        public CharacterEntity Body { get; private set; }

        public bool HasBodyReference => !_bodyReference.Value.Equals(NullBodyReference);

        public bool CanControlBody =>
            HasAuthority &&
            Body != null;

        public bool TrySpawnBody(
            NetworkObject networkPrefab,
            in Vector3 position,
            in Quaternion rotation,
            out CharacterEntity entity,
            bool isPlayer = false) {

            entity = null;
            if (!HasAuthority || HasBodyReference) {
                return false;
            }
            if (networkPrefab == null) {
                throw new ArgumentNullException(nameof(networkPrefab));
            }
            if (!networkPrefab.TryGetComponent<CharacterEntity>(out _)) {
                throw new ArgumentException($"{networkPrefab.name} has no {nameof(CharacterEntity)} component.", nameof(networkPrefab));
            }

            var bodyObject = networkObjectFactory.Instantiate(
                networkPrefab,
                ownerClientId: OwnerClientId,
                position: position,
                rotation: rotation
            );
            entity = bodyObject.GetComponent<CharacterEntity>();
            if (isPlayer) {
                bodyObject.SpawnAsPlayerObject(OwnerClientId, true);
            } else {
                bodyObject.Spawn(true);
            }

            _bodyReference.Value = bodyObject;
            Reconcile();
            return true;
        }

        public bool TryRecycleBody() {
            if (!CanControlBody) {
                return false;
            }

            var body = Body;
            _bodyReference.Value = NullBodyReference;
            Reconcile();
            body.NetworkObject.Despawn();
            return true;
        }

        public override void OnNetworkSpawn() {
            base.OnNetworkSpawn();
            _bodyReference.OnValueChanged += OnBodyReferenceChanged;
            _inputBinding = new CharacterInputBinding(this);
            Reconcile();
        }

        public override void OnNetworkDespawn() {
            _bodyReference.OnValueChanged -= OnBodyReferenceChanged;
            SetBody(null);
            base.OnNetworkDespawn();
        }

        protected override void OnOwnershipChanged(ulong previous, ulong current) {
            base.OnOwnershipChanged(previous, current);
            Reconcile();
        }

        protected virtual void Update() {
            if (_isBodyReferencePending) {
                Reconcile();
            }
        }

        protected virtual void OnBodyAssigned(CharacterEntity body) { }

        protected virtual void OnBodyReleased(CharacterEntity body) { }

        private void Reconcile() {
            if (!IsSpawned) {
                return;
            }

            var body = ResolveBody();
            SetBody(body);
            _inputBinding.SetEnabled(CanControlBody);
        }

        private CharacterEntity ResolveBody() {
            if (!HasBodyReference) {
                _isBodyReferencePending = false;
                return null;
            }
            if (!_bodyReference.Value.TryGet(out var bodyObject, NetworkManager)) {
                _isBodyReferencePending = true;
                return null;
            }

            _isBodyReferencePending = false;
            bodyObject.TryGetComponent(out CharacterEntity body);
            return body;
        }

        private void SetBody(CharacterEntity body) {
            if (ReferenceEquals(Body, body)) {
                return;
            }

            var previous = Body;
            if (previous != null) {
                OnBodyReleased(previous);
            }

            Body = body;
            _inputBinding.SetBody(body);
            if (body != null) {
                OnBodyAssigned(body);
            }

            BodyChanged?.Invoke(previous, body);
        }

        private void OnBodyReferenceChanged(NetworkObjectReference previous, NetworkObjectReference current) {
            Reconcile();
        }
    }
}
