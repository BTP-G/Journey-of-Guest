using Animancer;
using Xoderony;
using Xoderony.Movement;
using JoG.Health;
using Unity.Netcode;
using UnityEngine;
using VContainer;

namespace JoG.Character {

    public class CharacterEntity : Entity {

        public CharacterSpawner Spawner { get; set; }

        [Inject]
        internal IDelegateDispatcher<CharacterSpawnHandler> spawnHandlers;

        [Inject]
        internal IDelegateDispatcher<CharacterDespawnHandler> despawnHandlers;

        public Animator Animator { get; private set; }

        public AnimancerComponent Animancer { get; private set; }

        public CharacterBuffs Buffs { get; private set; }

        public HealthComponent Health { get; private set; }

        public CharacterModel Model { get; private set; }

        public CharacterMotor Motor { get; private set; }

        public InputBankHub InputBankHub { get; private set; }

        public HealthChangeRouter HealthChangeRouter { get; private set; }

        public HitRouter HitRouter { get; private set; }

        public Rigidbody Rigidbody { get; private set; }

        public override void OnNetworkSpawn() {
            base.OnNetworkSpawn();
            spawnHandlers.Handlers?.Invoke(this);
            Spawner.OnBodySpawn(this);
        }

        public override void OnNetworkDespawn() {
            base.OnNetworkDespawn();
            despawnHandlers.Handlers?.Invoke(this);
            Spawner.OnBodyDespawn(this);
        }

        protected override void OnSynchronize<T>(ref BufferSerializer<T> serializer) {
            var reference = new NetworkBehaviourReference(Spawner);
            serializer.SerializeNetworkSerializable(ref reference);
            reference.TryGet<CharacterSpawner>(out var spawner);
            Spawner = spawner;
            base.OnSynchronize(ref serializer);
        }

        protected override void Configure(IContainerBuilder builder) {
            base.Configure(builder);
            builder.RegisterInstance(this).As<Entity>();
            builder.RegisterBuildCallback(OnBuilt);
            builder.Register<InputBankHub>(Lifetime.Singleton).AsSelf();
            builder.RegisterInstance(Rigidbody = GetComponentInChildren<Rigidbody>());
            builder.RegisterInstance(Animator = GetComponentInChildren<Animator>());
            builder.RegisterInstance(Motor = GetComponentInChildren<CharacterMotor>());

            Animancer = GetComponentInChildren<AnimancerComponent>();
            if (Animancer != null) {
                builder.RegisterInstance(Animancer);
            }
        }

        private void OnBuilt(IObjectResolver container) {
            InputBankHub = container.Resolve<InputBankHub>();
            Health = container.Resolve<HealthComponent>();
            Buffs = container.Resolve<CharacterBuffs>();
            Model = container.Resolve<CharacterModel>();
            HealthChangeRouter = container.Resolve<HealthChangeRouter>();
            HitRouter = container.Resolve<HitRouter>();
        }

    }

}
