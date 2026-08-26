using Cysharp.Threading.Tasks;
using JoG.Character;
using JoG.GameplayEffects;
using JoG.Item;
using JoG.Localization;
using JoG.Modding;
using JoG.Player;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine.InputSystem;
using VContainer;
using VContainer.Unity;
using Xoderony.GameplayEffects;
using Xoderony.Logging;

namespace JoG {

    public sealed class ApplicationScope : LifetimeScope {
        private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(5);

        protected override void Awake() {
            autoRun = false;
            base.Awake();
            InitializeApplicationAsync(destroyCancellationToken).Forget(HandleInitializationException);
        }

        protected override void Configure(IContainerBuilder builder) {
            foreach (var map in InputSystem.actions.actionMaps) {
                builder.RegisterInstance(map).Keyed(map.name);
                foreach (var action in map.actions) {
                    builder.RegisterInstance(action).Keyed(action.name);
                }
            }
            builder.RegisterInstance(ItemDataDictionary.Shared).AsImplementedInterfaces();
            builder.RegisterInstance(GameplayEffectDefinitionRegistry.Shared).AsImplementedInterfaces();
            builder.RegisterInstance(PeriodicHealthChangeDefinitionDictionary.Shared).AsImplementedInterfaces();
            builder.RegisterInstance(CharacterDataDictionary.Shared).AsImplementedInterfaces();
            builder.Register<PlayerRegistry>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<SceneTransitionService>(Lifetime.Singleton).AsSelf();
            builder.Register<LocalResourcePackageLoader>(Lifetime.Singleton).As<IResourcePackageLoader>();
            builder.UseEntryPoints(static configuration => {
                configuration.Add<ModManager>();
                configuration.Add<DefaultPackageManager>();
                configuration.Add<DefaultLanguageBuilder>();
            });
        }

        private async UniTask InitializeApplicationAsync(CancellationToken cancellationToken) {
            var view = GetComponentInChildren<ApplicationInitializationView>(includeInactive: true);
            view.SetStatus("Building application services...");
            Build();
            view.SetStatus("Loading application data...");
            var modules = new List<IAsyncBootstrapModule>(Container.Resolve<IEnumerable<IAsyncBootstrapModule>>());
            var moduleTasks = new UniTask[modules.Count];
            for (var i = 0; i < modules.Count; i++) {
                moduleTasks[i] = modules[i].InitializeAsync(cancellationToken);
            }
            await UniTask.WhenAll(moduleTasks);
            cancellationToken.ThrowIfCancellationRequested();
            view.SetStatus("Loading main scene...");
            await LoadMainSceneWithRetryAsync(cancellationToken);
            view.Close();
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
