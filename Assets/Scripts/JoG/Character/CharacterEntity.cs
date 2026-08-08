using Animancer;
using JoG.Health;
using UnityEngine;
using VContainer;
using Xoderony;
using Xoderony.Movement;

namespace JoG.Character {

    public class CharacterEntity : Entity {

        [Inject]
        internal IDelegateDispatcher<CharacterSpawnHandler> spawnHandlers;

        [Inject]
        internal IDelegateDispatcher<CharacterDespawnHandler> despawnHandlers;

        public Animator Animator { get; private set; }

        public AnimancerComponent Animancer { get; private set; }

        public CharacterEffects Effects { get; private set; }

        public CharacterPeriodicHealthChanges PeriodicHealthChanges { get; private set; }

        public CharacterTimedEffects TimedEffects { get; private set; }

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
        }

        public override void OnNetworkDespawn() {
            base.OnNetworkDespawn();
            despawnHandlers.Handlers?.Invoke(this);
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
            Effects = container.Resolve<CharacterEffects>();
            PeriodicHealthChanges = container.Resolve<CharacterPeriodicHealthChanges>();
            TimedEffects = container.Resolve<CharacterTimedEffects>();
            Model = container.Resolve<CharacterModel>();
            HealthChangeRouter = container.Resolve<HealthChangeRouter>();
            HitRouter = container.Resolve<HitRouter>();
        }
    }
}
