using JoG.UI.Popup;
using JoG.Character;
using JoG.Gameplay.Effects;
using Xoderony.GameplayEffects;
using JoG.Item;
using JoG.Localization;
using JoG.Modding;
using JoG.Networking;
using JoG.Player;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Core;
using UnityEngine.InputSystem;
using VContainer;
using VContainer.Unity;

namespace JoG {

    public class RootScope : LifetimeScope {

        protected override void Configure(IContainerBuilder builder) {
            var unityServices = UnityServices.Instance;
            builder.RegisterInstance(unityServices);
            builder.RegisterInstance(unityServices.GetAuthenticationService());
            builder.RegisterInstance(unityServices.GetLobbyService());
            builder.RegisterInstance(unityServices.GetMatchmakerService());
            builder.RegisterInstance(unityServices.GetMultiplayerService());
            builder.RegisterInstance(unityServices.GetPlayerAccountService());
            builder.RegisterInstance(unityServices.GetQosService());
            builder.RegisterInstance(unityServices.GetRelayService());
            builder.RegisterInstance(new List<PlayerCharacterPrefabCard>());
            foreach (var map in InputSystem.actions.actionMaps) {
                builder.RegisterInstance(map).Keyed(map.name);
                foreach (var action in map.actions) {
                    builder.RegisterInstance(action).Keyed(action.name);
                }
            }
            builder.RegisterInstance(NetworkManager.Singleton);
            builder.Register<NetworkObjectFactory>(Lifetime.Singleton);
            builder.RegisterInstance(ItemDataDictionary.Shared).AsImplementedInterfaces();
            builder.RegisterInstance(GameplayEffectDefinitionRegistry.Shared).AsImplementedInterfaces();
            builder.RegisterInstance(PeriodicHealthChangeDefinitionDictionary.Shared).AsImplementedInterfaces();
            builder.RegisterInstance(CharacterDataDictionary.Shared).AsImplementedInterfaces();
            builder.RegisterInstance(FindFirstObjectByType<PopupManager>()).AsImplementedInterfaces();
            builder.RegisterInstance(FindFirstObjectByType<LoaderPopup>()).AsImplementedInterfaces();
            builder.RegisterInstance(FindFirstObjectByType<ConfirmPopup>()).AsImplementedInterfaces();
            builder.Register<UnityProfileService>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<AuthenticationController>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<PlayerRegistry>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.UseEntryPoints(configuration => {
                configuration.Add<ModManager>();
                configuration.Add<DefaultPackageManager>();
                configuration.Add<DefaultLanguageBuilder>();
                configuration.Add<SessionService>();
                configuration.Add<JoGApplication>();
            });
            builder.RegisterBuildCallback(container => {
                var manager = container.Resolve<NetworkManager>();
                var factory = container.Resolve<NetworkObjectFactory>();
                var playerPrefab = manager.NetworkConfig.PlayerPrefab.GetComponent<NetworkObject>();
                var handler = new NetworkPlayerPrefabHandler(manager, playerPrefab, container);
                factory.AddHandler(playerPrefab, handler);
            });
        }
    }
}
