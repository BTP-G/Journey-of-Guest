using Unity.Netcode;
using UnityEngine;
using UnityEngine.PlayerLoop;
using VContainer;
using Xoderony.Unity;

namespace JoG.Projectiles {

    /// <summary>射弹网络实体基类：同步 Owner、按 lifetime 超时销毁。弹种类型继承本类并自持行为。</summary>
    public abstract class ProjectileEntity : Entity {
        public float lifetime = 3f;
        private float _lifeEndTime;
        private Entity _owner;

        public Entity Owner => _owner;

        public void SetOwner(Entity owner) {
            _owner = owner;
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
        }

        protected override void Configure(IContainerBuilder builder) {
            base.Configure(builder);
            builder.RegisterInstance(this).As<Entity>();
        }

        protected override void OnSynchronize<T>(ref BufferSerializer<T> serializer) {
            base.OnSynchronize(ref serializer);
            if (serializer.IsWriter) {
                var writer = serializer.GetFastBufferWriter();
                writer.WriteValueSafe(_owner);
            } else {
                var reader = serializer.GetFastBufferReader();
                reader.ReadValueSafe(out _owner);
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
