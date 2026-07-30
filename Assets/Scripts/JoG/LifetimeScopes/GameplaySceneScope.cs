using JoG.Health;
using EditorAttributes;
using Xoderony.Unity;
using JoG.AI.Patrol;
using System.Collections.Generic;
using JoG.Chat;
using JoG.Gameplay;
using JoG.Networking;
using JoG.UI;
using MessagePipe;
using Unity.Netcode;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace JoG.LifetimeScopes {

    public class GameplaySceneScope : LifetimeScope {
        [Required] public WorldTooltip _worldTooltip;
        [Required] public ScreenTooltip _screenTooltip;
        [Required] public ChatBoxController _chatBoxController;
        [Required] public FloatingTextController _floatingTextController;
        [Required] public Billboarder billboarder;
        public PatrolService patrolService;
        public DifficultyManager difficultyManager;
        private readonly List<GenericPrefabInstanceHandler> _prefabHandlers = new();

        protected override void Configure(IContainerBuilder builder) {
            var options = builder.RegisterMessagePipe();
            options.InstanceLifetime = InstanceLifetime.Singleton;
            builder.RegisterMessageBroker<UIStateChangedMessage>(options);
            builder.RegisterMessageBroker<HealthChangeReport>(options);
            builder.RegisterMessageBroker<DeathMessage>(options);
            builder.RegisterEntryPoint<UIStateChangedHandler>();
            builder.RegisterEntryPoint<UnnamedMessageBroker>().AsSelf();
            builder.RegisterEntryPoint<ChatService>();
            builder.RegisterEntryPoint<HealthChangeRouter>().AsSelf();
            builder.RegisterEntryPoint<HitRouter>().AsSelf();
            builder.RegisterInstance(Camera.main).Keyed(Constants.Camera.MainCamera);
            builder.RegisterComponent(_floatingTextController);
            builder.RegisterComponent(_worldTooltip);
            builder.RegisterComponent(_screenTooltip);
            builder.RegisterComponent(_chatBoxController);
            builder.RegisterInstance(billboarder);
            builder.RegisterInstance(difficultyManager);
            builder.RegisterBuildCallback(OnBuilt);
            builder.RegisterDisposeCallback(OnDispose);
            builder.RegisterInstance(patrolService);
        }

        protected void Reset() {
            if (_worldTooltip == null) {
                _worldTooltip = FindFirstObjectByType<WorldTooltip>();
            }
            if (_screenTooltip == null) {
                _screenTooltip = FindFirstObjectByType<ScreenTooltip>();
            }
            if (_chatBoxController != null) {
                _chatBoxController = FindFirstObjectByType<ChatBoxController>();
            }
        }

        private void OnBuilt(IObjectResolver container) {
            var networkManager = container.Resolve<NetworkManager>();
            var factory = container.Resolve<NetworkObjectFactory>();
            foreach (var prefab in networkManager.NetworkConfig.Prefabs.Prefabs) {
                var prefabObject = prefab.Prefab.GetComponent<NetworkObject>();
                var handler = new GenericPrefabInstanceHandler(networkManager, prefabObject, this);
                if (factory.AddHandler(prefabObject, handler)) {
                    _prefabHandlers.Add(handler);
                }
            }
        }

        private void OnDispose(IObjectResolver container) {
            var factory = container.Resolve<NetworkObjectFactory>();
            foreach (var handler in _prefabHandlers) {
                factory.RemoveHandler(handler.prefab, handler);
            }
            _prefabHandlers.Clear();
        }
    }
}
