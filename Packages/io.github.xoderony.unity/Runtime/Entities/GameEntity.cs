using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Xoderony.ObjectPool.Generic;

namespace Xoderony {

    /// <summary>实体基础组件：组件容器、VContainer 子作用域与物理缓存；不依赖任何网络库。</summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-5000)]
    public class GameEntity : MonoBehaviour {

        protected IObjectResolver _container;

        private Collider[] _colliders;

        [field: SerializeReference, SerializeReferenceDropdown]
        public List<IComponent> Components { get; private set; }

        public Collider[] Colliders => _colliders;

        [field: SerializeField, HideInInspector]
        public LifetimeScope Parent { get; set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetComponent<T>(object key = null) where T : class {
            return _container.Resolve<T>(key);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetComponent<T>(out T component, object key = null) where T : class {
            return _container.TryResolve(out component, key);
        }

        protected virtual void Awake() {
            _colliders = GetComponentsInChildren<Collider>(true);
            _container = Parent.Container.CreateScope(Build);
        }

        protected virtual void OnDestroy() {
            _container.Dispose();
            _container = null;
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

        private void Build(IContainerBuilder builder) {
            builder.Register(typeof(DelegateChannel<>), Lifetime.Scoped).AsImplementedInterfaces();
            builder.RegisterInstance(gameObject);
            builder.RegisterBuildCallback(container => container.Inject(this));
            Configure(builder);
        }
    }
}
