using JoG.Core;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.PlayerLoop;
using VContainer;
using Xoderony.Unity;

namespace JoG.Projectiles {

    public class ProjectileEntity : Entity {
        public readonly PropertyHub propertyHub = new();
        public float lifetime = 3f;
        private float _lifeEndTime;
        private PropertyValue<Entity> _ownerEntity;

        public PropertyHub SetOwner(Entity owner) {
            _ownerEntity.value = owner;
            return propertyHub;
        }

        public override void OnNetworkSpawn() {
            base.OnNetworkSpawn();
            if (IsOwner) {
                _lifeEndTime = Time.time + lifetime;
                PreUpdateLoop<FixedUpdate.ScriptRunBehaviourFixedUpdate>.Register(OnPreFixedUpdate);
            }
        }

        public override void OnNetworkDespawn() {
            base.OnNetworkDespawn();
            PreUpdateLoop<FixedUpdate.ScriptRunBehaviourFixedUpdate>.Unregister(OnPreFixedUpdate);
            propertyHub.Reset();
        }

        protected new void Awake() {
            base.Awake();
            _ownerEntity = propertyHub.GetProperty<Entity>(Properties.Owner);
        }

        protected override void Configure(IContainerBuilder builder) {
            base.Configure(builder);
            builder.RegisterInstance(propertyHub);
        }

        protected override void OnSynchronize<T>(ref BufferSerializer<T> serializer) {
            base.OnSynchronize(ref serializer);
            if (serializer.IsWriter) {
                var writer = serializer.GetFastBufferWriter();
                writer.WriteValueSafe(_ownerEntity.value);
            } else {
                var reader = serializer.GetFastBufferReader();
                reader.ReadValueSafe(out _ownerEntity.value);
            }
        }

        private void OnPreFixedUpdate() {
            if (Time.time < _lifeEndTime) {
                return;
            }

            NetworkObject.DeferDespawn(4, true);
        }
    }
}
