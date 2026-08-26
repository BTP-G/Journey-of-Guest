using EditorAttributes;
using JoG.AI.Patrol;
using JoG.Chat;
using JoG.Combat;
using JoG.Gameplay;
using JoG.Health;
using JoG.Networking;
using JoG.UI;
using JoG.UI.Popup;
using MessagePipe;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Xoderony.Unity;

namespace JoG.LifetimeScopes {

    public class GameplayScope : LifetimeScope {
        [Required] public WorldTooltip _worldTooltip;
        [Required] public ScreenTooltip _screenTooltip;
        [Required] public ChatBoxController _chatBoxController;
        [Required] public FloatingTextController _floatingTextController;
        [Required] public Billboarder billboarder;
        public PatrolService patrolService;
        public DifficultyManager difficultyManager;

        protected override void Configure(IContainerBuilder builder) {
            builder.RegisterComponentInHierarchy<LoaderPopup>().AsSelf();
            builder.RegisterComponentInHierarchy<ConfirmPopup>().AsSelf();
            builder.RegisterComponentInHierarchy<ToastPopupController>().AsSelf();
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
            builder.Register<CombatDamage>(Lifetime.Singleton);
            builder.RegisterInstance(Camera.main).Keyed(Constants.Camera.MainCamera);
            builder.RegisterComponent(_floatingTextController);
            builder.RegisterComponent(_worldTooltip);
            builder.RegisterComponent(_screenTooltip);
            builder.RegisterComponent(_chatBoxController);
            builder.RegisterInstance(billboarder);
            builder.RegisterInstance(difficultyManager);
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
    }
}
