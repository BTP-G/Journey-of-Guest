using Xoderony.ObjectPool.Generic;
using Xoderony;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace JoG {

    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-5000)]
    public class Entity : NetworkBehaviour {
        internal static readonly Dictionary<ulong, Entity> IdToEntity = new();

        private IObjectResolver _container;
        private IEnumerable<INetworkSpawnHandler> _spawnHandlers;
        private IEnumerable<INetworkDespawnHandler> _depawnHandlers;
        private IEnumerable<INetworkOwnershipChangeHandler> _ownershipChangeHandlers;
        private IEnumerable<INetworkAuthorityChangedHandler> _authorityChangedHandlers;
        private IEnumerable<INetworkSynchronizeHandler> _synchronizeHandlers;
        private Collider[] _colliders;
        public static Dictionary<ulong, Entity>.ValueCollection Entities => IdToEntity.Values;

        public ulong Id => NetworkObjectId;

        [field: SerializeReference, SerializeReferenceDropdown]
        public List<IComponent> Components { get; private set; }

        public Collider[] Colliders => _colliders;

        [field: SerializeField, HideInInspector]
        public LifetimeScope Parent { get; set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity GetEntity(ulong id) {
            return IdToEntity[id];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetEntity(ulong id, out Entity entity) {
            return IdToEntity.TryGetValue(id, out entity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetComponent<T>(object key = null) where T : class {
            return _container.Resolve<T>(key);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetComponent<T>(out T component, object key = null) where T : class {
            return _container.TryResolve(out component, key);
        }

        public override void OnNetworkSpawn() {
            IdToEntity[NetworkObjectId] = this;
            foreach (var handler in _spawnHandlers) {
                handler.OnSpawn(IsOwner);
            }
            foreach (var handler in _ownershipChangeHandlers) {
                handler.OnGainedOwnership(IsOwner);
            }
            foreach (var handler in _authorityChangedHandlers) {
                handler.OnAuthorityChanged(HasAuthority);
            }
        }

        public override void OnNetworkDespawn() {
            IdToEntity.Remove(NetworkObjectId);
            foreach (var handler in _depawnHandlers) {
                handler.OnDespawn(IsOwner);
            }
            foreach (var handler in _ownershipChangeHandlers) {
                handler.OnLostOwnership(IsOwner);
            }
            foreach (var handler in _authorityChangedHandlers) {
                handler.OnAuthorityChanged(false);
            }
        }

        protected override void OnSynchronize<T>(ref BufferSerializer<T> serializer) {
            foreach (var handler in _synchronizeHandlers) {
                handler.OnSynchronize(ref serializer);
            }
        }

        protected override void OnOwnershipChanged(ulong previous, ulong current) {
            base.OnOwnershipChanged(previous, current);
            foreach (var handler in _ownershipChangeHandlers) {
                handler.OnLostOwnership(NetworkManager.LocalClientId == previous);
                handler.OnGainedOwnership(NetworkManager.LocalClientId == current);
            }
            foreach (var handler in _authorityChangedHandlers) {
                handler.OnAuthorityChanged(HasAuthority);
            }
        }

        protected virtual void Configure(IContainerBuilder builder) {
            var components = ListPool<IComponent>.Shared.Rent();
            GetComponentsInChildren(true, components);
            foreach (var component in components) {
                builder.RegisterInstance(component, component.GetType())
                    .AsImplementedInterfaces()
                    .Keyed(component.Key);
            }
            foreach (var component in Components) {
                builder.RegisterInstance(component, component.GetType())
                    .AsImplementedInterfaces()
                    .Keyed(component.Key);
            }
            builder.RegisterBuildCallback(container => {
                foreach (var component in components) {
                    container.Inject(component);
                }
                ListPool<IComponent>.Shared.Return(components);
                foreach (var component in Components) {
                    container.Inject(component);
                }
            });
        }

        protected void Awake() {
            _colliders = GetComponentsInChildren<Collider>(true);
            _container = Parent.Container.CreateScope(Build);
            _spawnHandlers = _container.Resolve<IEnumerable<INetworkSpawnHandler>>();
            _depawnHandlers = _container.Resolve<IEnumerable<INetworkDespawnHandler>>();
            _ownershipChangeHandlers = _container.Resolve<IEnumerable<INetworkOwnershipChangeHandler>>();
            _authorityChangedHandlers = _container.Resolve<IEnumerable<INetworkAuthorityChangedHandler>>();
            _synchronizeHandlers = _container.Resolve<IEnumerable<INetworkSynchronizeHandler>>();
        }

        protected new void OnDestroy() {
            base.OnDestroy();
            _container.Dispose();
            _container = null;
        }

        private void Build(IContainerBuilder builder) {
            builder.Register(typeof(DelegateChannel<>), Lifetime.Scoped).AsImplementedInterfaces();
            builder.RegisterInstance(gameObject);
            builder.RegisterInstance(NetworkObject);
            builder.RegisterBuildCallback(container => container.Inject(this));
            Configure(builder);
        }
    }
}
