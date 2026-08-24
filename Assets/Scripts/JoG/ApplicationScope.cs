using Cysharp.Threading.Tasks;
using JoG.Character;
using JoG.GameplayEffects;
using JoG.Item;
using JoG.Localization;
using JoG.Modding;
using JoG.Networking;
using JoG.Networking.P2P;
using JoG.Player;
using System;
using System.Collections.Generic;
using System.Threading;
using Unity.Netcode;
using Unity.Services.Core;
using UnityEngine.InputSystem;
using VContainer;
using VContainer.Unity;
using Xoderony.GameplayEffects;
using Xoderony.Logging;
using Xoderony.Networking;
using Xoderony.Networking.Transport;

namespace JoG {

    public sealed class ApplicationScope : LifetimeScope {
        private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(5);

        protected override void Awake() {
            autoRun = false;
            base.Awake();
            GetComponentInChildren<ApplicationInitializationView>(includeInactive: true).SetStatus("Initializing Unity Services...");
            InitializeApplicationAsync(destroyCancellationToken).Forget(HandleInitializationException);
        }

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
            builder.Register<UnityProfileService>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<AuthenticationController>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<PlayerRegistry>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<SceneTransitionService>(Lifetime.Singleton).AsSelf();
            builder.Register<NetworkMessageManager>(Lifetime.Singleton).AsSelf().As<INetworkMessageManager>();
            builder.Register<VContainerNetworkObjectFactory>(Lifetime.Singleton).AsSelf().As<INetworkObjectFactory>();
            builder.Register<NetworkObjectManager>(Lifetime.Singleton).AsSelf().As<INetworkObjectManager>();
            builder.Register<NetworkVariableModule>(Lifetime.Singleton).AsSelf();
            builder.Register<NetworkRpcModule>(Lifetime.Singleton).AsSelf();
            builder.UseEntryPoints(static configuration => {
                configuration.Add<ModManager>();
                configuration.Add<DefaultPackageManager>();
                configuration.Add<DefaultLanguageBuilder>();
                configuration.Add<SessionService>();
                configuration.Add<JoGApplication>();
                configuration.Add<SteamNetworkLobby>().AsSelf();
                configuration.Add<SteamNetworkTransport>().AsSelf().As<INetworkTransport>();
                configuration.Add<NetworkSession>().AsSelf().As<INetworkSession>();
                configuration.Add<SteamNetworkObjectIdAllocator>().AsSelf().As<INetworkObjectIdAllocator>();
                configuration.Add<SteamNetworkPeerConnector>().AsSelf();
                configuration.Add<P2PNetworkRuntime>().AsSelf();
            });
        }

        private async UniTask InitializeApplicationAsync(CancellationToken cancellationToken) {
            await InitializeUnityServicesWithRetryAsync(cancellationToken);

            GetComponentInChildren<ApplicationInitializationView>(includeInactive: true).SetStatus("Building application services...");
            Build();

            GetComponentInChildren<ApplicationInitializationView>(includeInactive: true).SetStatus("Loading application data...");
            var modules = new List<IAsyncBootstrapModule>(Container.Resolve<IEnumerable<IAsyncBootstrapModule>>());
            var moduleTasks = new UniTask[modules.Count];
            for (var i = 0; i < modules.Count; i++) {
                moduleTasks[i] = modules[i].InitializeAsync(cancellationToken);
            }

            await UniTask.WhenAll(moduleTasks);
            cancellationToken.ThrowIfCancellationRequested();

            GetComponentInChildren<ApplicationInitializationView>(includeInactive: true).SetStatus("Loading main scene...");
            await LoadMainSceneWithRetryAsync(cancellationToken);

            GetComponentInChildren<ApplicationInitializationView>(includeInactive: true).Close();
        }

        private async UniTask InitializeUnityServicesWithRetryAsync(CancellationToken cancellationToken) {
            while (UnityServices.State != ServicesInitializationState.Initialized) {
                cancellationToken.ThrowIfCancellationRequested();
                try {
                    await UnityServices.InitializeAsync();
                } catch (OperationCanceledException) {
                    throw;
                } catch (Exception exception) {
                    GetComponentInChildren<ApplicationInitializationView>(includeInactive: true).SetStatus("Unity Services initialization failed. Retrying in 5 seconds...");
                    this.LogException(exception);
                    await UniTask.Delay(RetryDelay, cancellationToken: cancellationToken);
                }
            }
        }

        private async UniTask LoadMainSceneWithRetryAsync(CancellationToken cancellationToken) {
            var sceneTransitionService = Container.Resolve<SceneTransitionService>();
            while (true) {
                cancellationToken.ThrowIfCancellationRequested();
                try {
                    await sceneTransitionService.LoadMainSceneAsync(cancellationToken);
                    return;
                } catch (OperationCanceledException) {
                    throw;
                } catch (Exception exception) {
                    GetComponentInChildren<ApplicationInitializationView>(includeInactive: true).SetStatus("Main scene loading failed. Retrying in 5 seconds...");
                    this.LogException(exception);
                    await UniTask.Delay(RetryDelay, cancellationToken: cancellationToken);
                }
            }
        }

        private void HandleInitializationException(Exception exception) {
            if (exception is OperationCanceledException) {
                return;
            }

            GetComponentInChildren<ApplicationInitializationView>(includeInactive: true).SetStatus($"Initialization failed.\n{exception}");
            this.LogException(exception);
        }
    }
}
